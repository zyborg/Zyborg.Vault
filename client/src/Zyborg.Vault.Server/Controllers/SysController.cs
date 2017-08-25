using System.Net;
using Microsoft.AspNetCore.Mvc;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server.Controllers
{
    [Route("v1/[controller]")]
    public class SysController : Controller
    {
        private MockServer _server;

        public SysController(MockServer server)
        {
            _server = server;
        }

        // TODO: just for debugging
        [HttpGet("settings")]
        public ServerSettings GetSettings()
        {
            return _server.Settings;
        }

        // GET api/values
        [HttpGet("health")]
        public HealthStatus GetHealth()
        {
            return new HealthStatus
            {
                Initialized = _server.Health.Initialized,
                Sealed = _server.Health.Sealed,
                Standby = _server.Health.Standby,

                ServerTimeUtc = _server.Health.ServerTimeUtc,
                Version = _server.Health.Version,

                ClusterId = _server.Health.ClusterId,
                ClusterName = _server.Health.ClusterName,
            };
        }

        [HttpGet("seal-status")]
        public SealStatus GetSealStatus()
        {
            return _server.GetSealStatus() ?? throw new VaultServerException(
                    HttpStatusCode.BadRequest,
                    "server is not yet initialized");
        }

        [HttpGet("key-status")]
        public KeyStatus GetKeyStatus()
        {
            return _server.GetKeyStatus() ?? throw new VaultServerException(
                    HttpStatusCode.ServiceUnavailable,
                    "Vault is sealed");
        }

        [HttpGet("init")]
        public InitializationStatus GetInitStatus()
        {
            return _server.GetInitializationStatus();
        }

        [HttpPut("init")]
        public InitializationResponse DoInit([FromBody]InitializationRequest requ)
        {
            return _server.Initialize(requ.SecretShares, requ.SecretThreshold)
                    ?? throw new VaultServerException(
                            HttpStatusCode.BadRequest,
                            "Vault is already initialized");
        }
    }
}