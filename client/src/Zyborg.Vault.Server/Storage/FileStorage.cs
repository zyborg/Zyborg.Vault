using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zyborg.Vault.Server.Util;

namespace Zyborg.Vault.Server.Storage
{
    /// <summary>
    /// A <see cref="IStorage">Storage</see> implementation that uses the file system
    /// to persist its stored content.
    /// </summary>
    /// <remarks>
    /// This implementation supports durable storage in the form of directories and
    /// files that map directly from the argument paths.  It preserves the trait of
    /// supporting distinct namespaces for container (directory) and leaf (file)
    /// nodes at any given path by using fixed and distinct prefixes to segment
    /// names when mapped to files on disk.
    /// <para>
    /// Configuration settings that control this implementations behavior are
    /// defined in <see cref="FileStorageSettings"/>.
    /// </para>
    /// </remarks>    
    [ConfiguredBy(typeof(FileStorageSettings))]
    public class FileStorage : IStorage
    {
        public const string DirPrefix = "%";
        public const string FilePrefix = "_";

        private IConfiguration _config;
        private FileStorageSettings _settings;

        private string _rootPath;
        private DirectoryInfo _rootDir;

        public FileStorage(Func<IStorage, IConfiguration> configGetter)
        {
            _config = configGetter(this);
            _settings = _config.Get<FileStorageSettings>();

            if (string.IsNullOrEmpty(_settings.Path))
                throw new ArgumentNullException("required storage setting missing", nameof(_settings.Path));

            _rootPath = Path.GetFullPath(Path.Combine(_settings.Path, "TEMPORARY"));
            if (!Directory.Exists(_rootPath))
                if (!Directory.CreateDirectory(_rootPath).Exists)
                    throw new InvalidOperationException("failed to initialize file storage");
            
            _rootDir = new DirectoryInfo(_rootPath);
        }

        private string NormalizeDirPath(string path)
        {
            path = DirPrefix + PathMap<object>.NormalizePath(path).Replace("/", $"/{DirPrefix}");
            path = Path.GetFullPath(Path.Combine(_rootPath, path));
            return path;
        }

        private string NormalizeFilePath(string path)
        {
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
            path = Path.GetFullPath(Path.Combine(_rootPath, path));
            return path;
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            path = NormalizeDirPath(path);

            if (!Directory.Exists(path))
                return await Task.FromResult(default(IEnumerable<string>));

            IEnumerable<string> list = Directory.GetDirectories(path, DirPrefix + "*")
                    .Select(x => Path.GetFileName(x).Substring(1) + "/")
                    .Concat(Directory.GetFiles(path, FilePrefix + "*")
                            .Select(x => Path.GetFileName(x).Substring(1)))
                    .OrderBy(x => x);

            return await Task.FromResult(list);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            path = NormalizeFilePath(path);
            return await Task.FromResult(File.Exists(path));
        }

        public async Task<string> ReadAsync(string path)
        {
            path = NormalizeFilePath(path);

            if (!File.Exists(path))
                return await Task.FromResult(default(string));

            return await Task.FromResult(File.ReadAllText(path));
        }

        public async Task WriteAsync(string path, string value)
        {
            path = NormalizeFilePath(path);

            string parent = Path.GetDirectoryName(path);
            if (!Directory.Exists(parent))
                if (!Directory.CreateDirectory(parent).Exists)
                    throw new InvalidOperationException("failed to create parent path");

            File.WriteAllText(path, value);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string path)
        {
            path = NormalizeFilePath(path);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            await Task.CompletedTask;
        }
 
        /// <summary>
        /// Defines the configurations settings for a <see cref="FileStorage"/> instance.
        /// </summary>
        public class FileStorageSettings
        {
            /// <summary>
            /// Specifies the root path where <see cref="FileStorage"/> with persist its
            /// stored content.  This is a required setting and a missing or invalid value
            /// will cause the storage instance to fail initial startup.
            /// </summary>
            [Required]
            public string Path
            { get; set; }
        }
    }
}