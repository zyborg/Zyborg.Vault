using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Zyborg.Vault.Model;
using Zyborg.Vault.MockServer.Protocol;
using Zyborg.Vault.MockServer.Storage;

namespace Zyborg.Vault.MockServer.Secret
{
    public class GenericSecretBackend : ISecretBackend
    {
        private IStorage _storage;

        public GenericSecretBackend(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            return await _storage.ListAsync(path);
        }

        public async Task<string> ReadAsync(string path)
        {
            return await _storage.ReadAsync(path);
        }

        public async Task WriteAsync(string path, string payload)
        {
            await _storage.WriteAsync(path, payload);
        }

        public async Task DeleteAsync(string path)
        {
            await _storage.DeleteAsync(path);
        }
    }
}