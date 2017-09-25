using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Storage
{
    public class PathMap<T>
    {
        private ConcurrentDictionary<string, List<string>> _paths =
                new ConcurrentDictionary<string, List<string>>();
        private ConcurrentDictionary<string, T> _values =
                new ConcurrentDictionary<string, T>();

        private object _writeLock = new object();

        private static readonly Regex DotSegmentsRegex = new Regex("(^|\\/)\\.+(\\/|$)");
        private static readonly Regex LeadingTrailingTrimRegex = new Regex("(^[\\s\\/]+|[\\s\\/]$)");
        private static readonly Regex MultipleSlashesRegex = new Regex("\\/{2}");

        public static string NormalizePath(string path)
        {
            if (path == null)
                return null;
            
            // TODO:  this is the naive way to do all these,
            // optimize this area since it gets used a lot!

            if (DotSegmentsRegex.IsMatch(path))
                throw new ArgumentException("illegal path segments found", nameof(path));

            // Remove leading/trailing whitespace and slashes and duplicate slashes within
            return MultipleSlashesRegex.Replace(
                    LeadingTrailingTrimRegex.Replace(path, ""), "/");
        }

        public void Set(string path, T value)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = NormalizePath(path);
            if (path.Length == 0)
                throw new ArgumentException("empty path is not allowed", nameof(path));

            var segs = path.Split("/");
            var dirs = segs.Length - 1;

            lock (_writeLock) // We need to update the paths and value atomically
            {
                // Parent paths always end in a '/' including the root

                var parent = ""; // start with the root (no segments)
                var segNdx = 0;

                // Here we just build or update our hierarchy of path dirs
                while (segNdx <= dirs)
                {
                    // Make sure next parent path ends in a '/'
                    parent = $"{parent}/";
                    _paths.AddOrUpdate(parent,
                            (k) => new List<string> { segs[segNdx] },
                            (k, l) => { l.Add( segs[segNdx]); return l; });
                    
                    // Add the next segment in the path
                    parent = $"{parent}{segs[segNdx++]}";
                }

                // At this point parent is equal to the full path including the leaf segment
                _values[parent] = value;
            }
        }

        public void Delete(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = NormalizePath(path);
            if (path.Length == 0)
                throw new ArgumentException("empty path is not allowed", nameof(path));

            var segs = path.Split("/");
            var dirs = segs.Length - 1;

            lock (_writeLock)
            {
                // Parent paths always end in a '/' including the root

                var parent = ""; // start with the root (no segments)
                var segNdx = 0;

                // In this case we're updating our hierarchy of path
                // dirs by removing the parent dirs if they become empty
                while (segNdx <= dirs)
                {
                    // Make sure next parent path ends in a '/'
                    parent = $"{parent}/";
                    if (_paths.TryGetValue(parent, out var children) && children.Contains(parent))
                        if (children.Count == 1)
                            _paths.TryRemove(parent, out children);
                        else
                            children.Remove(segs[segNdx]);

                    // Add the next segment in the path
                    parent = $"{parent}{segs[segNdx++]}";
                }

                // At this point parent is equal to the full path including the leaf segment
                _values.TryRemove(parent, out var value);
            }
        }

        public IEnumerable<string> ListPaths()
        {
            return _values.Keys;
        }

        public IEnumerable<string> ListChildren(string parentPath)
        {
            if (parentPath == null)
                throw new ArgumentNullException(nameof(parentPath));
            
            parentPath = NormalizePath(parentPath);
            if (parentPath.Length > 0)
                parentPath += "/";

            var value = (IEnumerable<string>)null;
            if (_paths.TryGetValue("/" + parentPath, out var listValue))
                value = listValue;

            return value;
        }

        public bool Exists(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = NormalizePath(path);
            if (path.Length == 0)
                throw new ArgumentException("empty path is not allowed", nameof(path));

            return _values.ContainsKey("/" + path);
        }

        public T Get(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = NormalizePath(path);
            if (path.Length == 0)
                throw new ArgumentException("empty path is not allowed", nameof(path));

            return _values.TryGetValue("/" + path, out var value)
                    ? value
                    : default(T);
        }
    }
}