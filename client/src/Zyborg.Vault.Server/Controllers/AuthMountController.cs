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

namespace Zyborg.Vault.Server.Controllers
{
    [Route("v1/auth/{*mount}")]
    public class AuthMountController : MountControllerBase
    {

        private MockServer _server;

        public AuthMountController(MockServer server)
        {
            _server = server;
        }

        [HttpList(Name = "ListAuth")]
        public async Task<IActionResult> ListAsync([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveAuthMount(mount);
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

        [HttpGet(Name = "ReadAuth", Order = int.MaxValue)]
        public async Task<IActionResult> ReadAsync([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveAuthMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");

            try
            {
                var dataSer = await backend.ReadAsync(path);
                var obj = default(object);
                if (!string.IsNullOrEmpty(dataSer))
                    obj = JsonConvert.DeserializeObject(dataSer);
                
                return base.Ok(
                        new ReadResponse<object>
                        {
                            Data = obj,
                        });
            }
            catch (Exception ex)
            {
                return await DecodeException(ex);
            }
        }

        [HttpPut(Name = "WriteAuth", Order = int.MaxValue)]
        [HttpPost(Name = "PostAuth", Order = int.MaxValue)]
        public async Task<IActionResult> WriteAsync([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveAuthMount(mount);
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

            try
            {
                await backend.WriteAsync(path, json);
                
                return base.NoContent();
            }
            catch (Exception ex)
            {
                return await DecodeException(ex);
            }
        }

        [HttpDelete(Name = "DeleteAuth", Order = int.MaxValue)]
        public  async Task<IActionResult> DeleteAsync([FromRoute]string mount)
        {
            var (backend, path) = _server.ResolveAuthMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");

            try
            {
                await backend.DeleteAsync(path);
                
                return base.NoContent();
            }
            catch (Exception ex)
            {
                return await DecodeException(ex);
            }
        }
    }
}