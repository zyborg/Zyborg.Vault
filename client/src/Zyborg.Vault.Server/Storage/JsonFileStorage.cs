using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zyborg.Vault.Server.Storage
{
    /// <summary>
    /// A <see cref="IStorage">Storage</see> implementation that uses a single
    /// JSON file to persist its stored content.
    /// </summary>
    /// <remarks>
    /// This implements supports durable storage of its content by persisting
    /// to a single JSON file, mapping directories to intermediate JSON objects
    /// and files to string-valued properties.  It preserves the trait of
    /// supporting distinct namespaces for container (directory) and leaf (file)
    /// nodes at any given path by using fixed and distinct prefixes to segment
    /// names when mapped to property names on objects.
    /// <para>
    /// This Storage implementation may not be the most efficient or performant
    /// however it is very useful and ideal when you need to setup or build up
    /// a particular set of stored data, and then snapshot that data so that it
    /// can be replayed repeatedly to produce a predictable starting point, for
    /// example, in the case of unit or integration testing.  In this example,
    /// you could launch a new instance of Mock Server using this storage
    /// implementation, write some predefined arrangement of data to the server,
    /// then copy the final JSON file as a starting snapshot for future replay.
    /// </para><para>
    /// Configuration settings that control this implementations behavior are
    /// defined in <see cref="JsonFileStorageSettings"/>.
    /// </para>
    /// </remarks>
    public class JsonFileStorage : IStorage
    {
        public const string DirPrefix = "%";
        public const string FilePrefix = "_";

        private IConfiguration _config;
        private JsonFileStorageSettings _settings;

        private string _path;
        private JObject _data;
        private object _sync = new object();

        public JsonFileStorage(Func<IStorage, IConfiguration> configGetter)
        {
            _config = configGetter(this);
            _settings = _config.Get<JsonFileStorageSettings>();

            if (string.IsNullOrEmpty(_settings.Path))
                throw new ArgumentNullException("required storage setting missing", nameof(_settings.Path));

            _path = Path.GetFullPath(_settings.Path);

            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                _data = JsonConvert.DeserializeObject<JObject>(json);
            }
            else
            {
                _data = JObject.Parse("{}");
                File.WriteAllText(_path, JsonConvert.SerializeObject(_data, Formatting.Indented));
            }
        }

        private string NormalizeDirJsonPath(string path)
        {
            if (path.Contains("'"))
                throw new InvalidDataException("path cannot contain quotes");

            path = PathMap<object>.NormalizePath(path);

            path = $"['{DirPrefix}" + path.Replace("/", $"'].['{DirPrefix}") + "']";
            return path;
        }

        private string NormalizeFileJsonPath(string path)
        {
            if (path.Contains("'"))
                throw new InvalidDataException("path cannot contain quotes");

            path = PathMap<object>.NormalizePath(path);

            // Start by assuming a single path segment, so just the file and no dir
            var dir = string.Empty;
            var file = path;
            var ndx = path.LastIndexOf("/");
            if (ndx > 0)
            {
                // Extract out the dir path and the file segment
                dir = path.Substring(0, ndx);
                file = path.Substring(ndx + 1);
            }

            path = $"['{DirPrefix}" + dir.Replace("/", $"'].['{DirPrefix}") + $"'].['{FilePrefix}" + file + "']";
            return path;
        }

        private string NormalizeFileSlashPath(string path)
        {
            if (path.Contains("'"))
                throw new InvalidDataException("path cannot contain quotes");

            path = PathMap<object>.NormalizePath(path);

            // Start by assuming a single path segment, so just the file and no dir
            var dir = string.Empty;
            var file = path;
            var ndx = path.LastIndexOf("/");
            if (ndx > 0)
            {
                // Extract out the dir path and the file segment
                dir = path.Substring(0, ndx);
                file = path.Substring(ndx + 1);
            }

            path = DirPrefix + dir.Replace("/", $"/{DirPrefix}") + $"/{FilePrefix}" + file;
            return path;
        }


        private async Task SaveJsonFileAsync()
        {
            await File.WriteAllTextAsync(_path, JsonConvert.SerializeObject(_data, Formatting.Indented));
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            path = NormalizeDirJsonPath(path);
            var obj = (JObject)_data.SelectToken(path);
            
            IEnumerable<string> list = null;
            if (obj != null)
                list = obj.Properties().Select(x => x.Type == JTokenType.Object
                        ? x.Name.Substring(1) + "/"
                        : x.Name.Substring(1));

            return await Task.FromResult(list);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            path = NormalizeFileJsonPath(path);

            var  node = _data.SelectToken(path);

            return await Task.FromResult(node != null && node.Type == JTokenType.String);
        }

        public async Task<string> ReadAsync(string path)
        {
            path = NormalizeFileJsonPath(path);

            var  node = _data.SelectToken(path);

            return await Task.FromResult(node == null || node.Type != JTokenType.String
                    ? null
                    : node.Value<string>());
        }

        public async Task WriteAsync(string path, string value)
        {
            path = NormalizeFileSlashPath(path);

            var dir = string.Empty;
            var file = path;
            var sep = file.LastIndexOf("/");
            if (sep > 0)
            {
                dir = file.Substring(0, sep);
                file = file.Substring(sep + 1);
            }

            lock (_sync)
            {
                var dirNode = _data;
                foreach (var pathSegment in dir.Split("/"))
                {
                    var token = dirNode.GetValue(pathSegment);
                    if (token != null)
                    {
                        if (token.Type != JTokenType.Object)
                            throw new InvalidOperationException(
                                    "path intermediate node is not an expected container");
                        dirNode = (JObject)token;
                    }
                    else
                    {
                        var newNode = JObject.Parse("{}");
                        dirNode.Add(pathSegment, newNode);
                        dirNode = newNode;
                    }
                }

                var prop = dirNode.Property(file);
                if (prop == null)
                    dirNode.Add(new JProperty(file, value));
                else
                    prop.Value = value;

                // Can't await inside a lock
                SaveJsonFileAsync().Wait();
            }

            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string path)
        {
            path = NormalizeFileJsonPath(path);

            lock (_sync)
            {
                var token = _data.SelectToken(path);
                if (token != null && token.Parent?.Type == JTokenType.Property)
                {
                    var prop = (JProperty)token.Parent;
                    prop.Remove();

                    // Can't await inside a lock
                    SaveJsonFileAsync().Wait();
                }
            }

            await Task.CompletedTask;
        }
 
        /// <summary>
        /// Defines the configurations settings for a <see cref="JsonFileStorage"/> instance.
        /// </summary>
        public class JsonFileStorageSettings
        {
            /// <summary>
            /// Specifies the path where <see cref="JsonFileStorage"/> with store its JSON
            /// content.  This is a required setting and a missing or invalid value
            /// will cause the storage instance to fail initial startup.
            /// </summary>
            [Required]
            public string Path
            { get; set; }
        }
    }
}