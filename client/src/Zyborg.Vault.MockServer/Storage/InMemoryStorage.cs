using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Storage
{
    /// <summary>
    /// A <see cref="IStorage">Storage</see> implementation that is backed by an
    /// in-memory <see cref="PathMap{T}">PathMap</see> with string values.
    /// </summary>
    /// <remarks>
    /// As this is an in-memory only implementation, it will not persist or preserve
    /// any of its stored content to durable storage and therefore will be reset
    /// after each restart of its hosting server.
    /// </remarks>
    public class InMemoryStorage : PathMap<string>, IStorage
    {
        public Task<IEnumerable<string>> ListAsync(string path)
        {
            return Task.FromResult(base.ListChildren(path));
        }

        public Task<bool> ExistsAsync(string path)
        {
            return Task.FromResult(base.Exists(path));
        }

        public Task<string> ReadAsync(string path)
        {
            return Task.FromResult(base.Get(path));
        }

        public Task WriteAsync(string path, string value)
        {
            base.Set(path, value);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string path)
        {
            base.Delete(path);
            return Task.CompletedTask;
        }
    }
}