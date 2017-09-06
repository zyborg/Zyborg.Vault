using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zyborg.Vault.Server.Storage
{
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