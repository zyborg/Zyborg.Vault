using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Zyborg.Vault.Model;
using Zyborg.Vault.MockServer.Auth;
using Zyborg.Vault.MockServer.Util;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.Controllers
{
    [Route("v1/auth/{*mount}")]
    public class AuthMountController : MountControllerBase
    {

        protected AuthContext _auth;
        protected MockServer _server;

        public AuthMountController(MockServer server)
        {
            _server = server;
        }

        [HttpList(Name = "ListAuth")]
        public async Task<IActionResult> ListAsync([FromRoute]string mount)
        {
            _server.AssertAuthorized(this);

            var (backend, path) = _server.ResolveAuthMount(mount);
            if (backend == null)
                throw new VaultServerException(
                        HttpStatusCode.NotFound,
                        $"no handler for route '{mount}'");

            try
            {
                RememberMe();
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
                RememberMe();
                var ret = await backend.ReadAsync(path);
                // if (ret is NoContentResponse)
                if (ret == null)
                    return base.NoContent();

                var resp = new ReadResponse<object>();
                if (ret is AuthInfo auth)
                    resp.Auth = auth;
                else
                    resp.Data = ret;

                return base.Ok(resp);
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
                RememberMe();
                var ret = await backend.WriteAsync(path, json);
                // if (ret is NoContentResponse)
                if (ret == null)
                    return base.NoContent();

                var resp = new ReadResponse<object>();
                if (ret is AuthInfo auth)
                    resp.Auth = auth;
                else
                    resp.Data = ret;

                return base.Ok(resp);
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
                RememberMe();
                await backend.DeleteAsync(path);
                
                return base.NoContent();
            }
            catch (Exception ex)
            {
                return await DecodeException(ex);
            }
        }

        private void RememberMe()
        {
            HttpContext.Items[nameof(AuthMountController)] = this;
        }

        public static Controller From(HttpContext http)
        {
            return http.Items[nameof(AuthMountController)] as AuthMountController;
        }
    }
}