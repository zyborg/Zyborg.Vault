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
    public class SecretMountController : Controller
    {
        private MockServer _server;

        public SecretMountController(MockServer server)
        {
            _server = server;
        }

        [HttpList]
        [SuccessType(typeof(ReadResponse<KeysData>))]
        public async Task<IActionResult> ListAsync([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveSecretMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");
            
            var list = await backend.List(path);
            if (list == null)
                throw new VaultServerException(HttpStatusCode.NotFound);

            return base.Ok(
                    new ReadResponse<KeysData>
                    {
                        Data = new KeysData
                        {
                            Keys = list.ToArray(),
                        }
                    });
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
            
            var dataSer = await backend.Read(path);
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

            await backend.Write(path, json);

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
            
            await backend.Delete(path);

            return NoContent();
        }
    }
}