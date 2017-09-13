using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Zyborg.Vault.Model;
using Zyborg.Vault.Server.Protocol;
using Zyborg.Vault.Server.Secret;

namespace Zyborg.Vault.Server.Controllers
{
    [Route("v1/{*mount}")]
    public class SecretMountController : MountControllerBase
    {
        private MockServer _server;

        public SecretMountController(MockServer server)
        {
            _server = server;
        }

        [HttpList(Name = "ListSecret")]
        [SuccessType(typeof(ReadResponse<KeysData>))]
        public async Task<IActionResult> ListAsync([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveSecretMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");
            
            try
            {
                var list = await backend.ListAsync(path);

                return base.Ok(
                        new ReadResponse<KeysData>
                        {
                            Data = new KeysData
                            {
                                Keys = list?.ToArray(),
                            }
                        });
            }
            catch (Exception ex)
            {
                return await DecodeException(ex);
            }
        }

        [HttpGet(Name = "ReadSecret", Order = int.MaxValue)]
        [SuccessType(typeof(ReadResponse<object>))]
        public async Task<IActionResult> Read([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveSecretMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");
            
            var dataSer = await backend.ReadAsync(path);
            if (dataSer == null)
                throw new VaultServerException(HttpStatusCode.NotFound);

            var obj = JsonConvert.DeserializeObject(dataSer);

            return base.Ok(
                    new ReadResponse<object>
                    {
                        Data = obj,
                    });
        }

        [HttpPut(Name = "WriteSecret", Order = int.MaxValue)]
        [HttpPost(Name = "PostSecret", Order = int.MaxValue)]
        public async Task<IActionResult> Write([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveSecretMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");
            

            // Decode the bytes as UTF8 string
            string json;
            using (var b = new StreamReader(this.Request.Body, Encoding.UTF8))
            {
                json = b.ReadToEnd();
            }

            // Make sure the JSON is legal
            var obj = JsonConvert.DeserializeObject(json);

            await backend.WriteAsync(path, json);

            return NoContent();
        }

        [HttpDelete(Name = "DeleteSecret", Order = int.MaxValue)]
        public async Task<IActionResult> Delete([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveSecretMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");
            
            await backend.DeleteAsync(path);

            return NoContent();
        }
    }
}