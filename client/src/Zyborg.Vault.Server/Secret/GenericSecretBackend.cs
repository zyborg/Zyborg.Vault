using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Zyborg.Vault.Model;
using Zyborg.Vault.Server.Protocol;
using Zyborg.Vault.Server.Storage;

namespace Zyborg.Vault.Server.Secret
{
    public class GenericSecretBackend : ISecretBackend
    {
        private IStorage _storage;

        // private Dictionary<string, string> _secrets = new Dictionary<string, string>
        // {
        //     ["my-secret1"] = JsonConvert.SerializeObject(new { foo = "bar" }),
        //     ["my-secret2"] = JsonConvert.SerializeObject(new { foo = "baz" }),
        // };

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



        // [Http]
        // [HttpList("{path?}", Name = "ListGenericSecret")]
        // public ReadResponse<KeysData> List(
        //         [FromRoute]string mountName,
        //         [FromRoute]string path,
        //         [FromWrapTtlHeader]string wrapTtl)
        // {
        //     string[] keys = null;
        //     if (string.IsNullOrEmpty(path))
        //         keys = _secrets.Keys.ToArray();
        //     else if (_secrets.ContainsKey(path))
        //         keys = JsonConvert.DeserializeObject<Dictionary<string, object>>(
        //                 _secrets[path]).Keys.ToArray();

        //     if ((keys?.Length).GetValueOrDefault() == 0)
        //         throw new VaultServerException(HttpStatusCode.NotFound);

        //     return new ReadResponse<KeysData>
        //     {
        //         Data = new KeysData
        //         { 
        //             Keys = keys,
        //         },
        //     };
        // }

        // [HttpGet("{path}", Name = "ReadGenericSecret")]
        // public ReadResponse<object> Read(
        //         [FromRoute]string mountName,
        //         [FromRoute]string path,
        //         [FromWrapTtlHeader]string wrapTtl)
        // {
        //     object data;
        //     if (string.IsNullOrEmpty(path) || !_secrets.ContainsKey(path))
        //         throw new VaultServerException(HttpStatusCode.NotFound);
        //     else
        //         data = JsonConvert.DeserializeObject(_secrets[path]);

        //     return new ReadResponse<object>
        //     {
        //         Data = data,
        //     };
        // }
    }
}