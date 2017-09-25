using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Storage
{
    public class StorageWrapper : IStorage
    {
        private IStorage _target;
        private string _prefix;

        public StorageWrapper(IStorage target, string prefix)
        {
            _target = target;
            _prefix = prefix;
        }

        public async Task<bool> ExistsAsync(string path)
        {
            return await _target.ExistsAsync($"{_prefix}/{path}");
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            return await _target.ListAsync($"{_prefix}/{path}");
        }

        public async Task<string> ReadAsync(string path)
        {
            return await _target.ReadAsync($"{_prefix}/{path}");
        }

        public async Task WriteAsync(string path, string value)
        {
            await _target.WriteAsync($"{_prefix}/{path}", value);
        }

        public async Task DeleteAsync(string path)
        {
            await _target.DeleteAsync($"{_prefix}/{path}");
        }
    }
}