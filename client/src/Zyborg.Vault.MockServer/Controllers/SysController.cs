using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Model;
using Zyborg.Vault.MockServer.Auth;
using Zyborg.Vault.MockServer.Policy;

namespace Zyborg.Vault.MockServer.Controllers
{
    [Route("v1/sys")]
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
            _server.AssertAuthorized(this, isSudo: true);

            return _server.Settings;
        }

        /// <summary>
        /// The /sys/health endpoint is used to check the health status of Vault.
        /// </summary>
        /// <param name="standbyok">Specifies if being a standby should still return the
        ///     active status code instead of the standby status code. This is useful when
        ///     Vault is behind a non-configurable load balance that just wants a 200-level
        ///     response.</param>
        /// <param name="activecode">Specifies the status code that should be returned for
        ///     an active node.</param>
        /// <param name="standbycode"Specifies the status code that should be returned for
        ///     a standby node.</param>
        /// <param name="sealedcode">Specifies the status code that should be returned for
        ///     a sealed node.</param>
        /// <param name="uninitcode">Specifies the status code that should be returned for
        ///     a uninitialized node.</param>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// 
        /// <para>
        /// This endpoint returns the health status of Vault. This matches the semantics of
        /// a Consul HTTP health check and provides a simple way to monitor the health of a
        /// Vault instance.
        /// </para><para>
        /// The default status codes are:
        /// <list>
        /// <item>200 if initialized, unsealed, and active</item>
        /// <item>429 if unsealed and standby</item>
        /// <item>501 if not initialized</item>
        /// <item>503 if sealed</item>
        /// </list>
        /// </para>
        /// </remarks>
        [HttpHead("health")]
        [HttpGet("health")]
        public IActionResult GetHealth(
                [FromQuery]bool standbyok = false,
                [FromQuery]int activecode = 200,
                [FromQuery]int standbycode = 429,
                [FromQuery]int sealedcode = 503,
                [FromQuery]int uninitcode = 501)
        {
            var http = HttpContext;
            var cc = base.ControllerContext;
            var uh = base.Url;
            var rd = base.RouteData;

            var status = new HealthStatus
            {
                Initialized = _server.Health.Initialized,
                Sealed = _server.Health.Sealed,
                Standby = _server.Health.Standby,

                ServerTimeUtc = _server.Health.ServerTimeUtc,
                Version = _server.Health.Version,

                ClusterId = _server.Health.ClusterId,
                ClusterName = _server.Health.ClusterName,
            };

            var statusCode = activecode;
            if (!status.Initialized)
                statusCode = uninitcode;
            else if (status.Sealed)
                statusCode = sealedcode;
            else if (status.Standby && !standbyok)
                statusCode = standbycode;

            return base.StatusCode(statusCode, status);
        }

        /// <summary>
        /// This endpoint returns the initialization status of Vault.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// </remarks>
        [HttpGet("init")]
        public InitializationStatus GetInitStatus()
        {
            return _server.GetInitializationStatus();
        }

        /// <summary>
        /// This endpoint initializes a new Vault. The Vault must not have been
        /// previously initialized. The recovery options, as well as the stored
        /// shares option, are only available when using Vault HSM.
        /// </summary>
        /// <param name="requ"></param>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// </remarks>
        [HttpPut("init")]
        public InitializationResponse StartInit([FromBody]InitializationRequest requ)
        {
            return _server.Initialize(requ.SecretShares, requ.SecretThreshold)
                    ?? throw new VaultServerException(
                            HttpStatusCode.BadRequest,
                            "Vault is already initialized");
        }

        /// <summary>
        /// Used to check the seal status of a Vault.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// 
        /// This endpoint returns the seal status of the Vault. This is an unauthenticated endpoint.
        /// </remarks>
        [HttpGet("seal-status")]
        public SealStatus GetSealStatus()
        {
            return _server.GetSealStatus() ?? throw new VaultServerException(
                    HttpStatusCode.BadRequest,
                    "server is not yet initialized");
        }

        /// <summary>
        /// Used to query info about the current encryption key of Vault.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This endpoint is authenticated, but SUDO is NOT required.</i></b></para>
        /// 
        /// This endpoint returns information about the current encryption key used by Vault.
        /// </remarks>
        [HttpGet("key-status")]
        public KeyStatus GetKeyStatus()
        {
            _server.AssertAuthorized(this);

            return _server.GetKeyStatus() ?? throw new VaultServerException(
                    HttpStatusCode.ServiceUnavailable,
                    "Vault is sealed");
        }

        /// <summary>
        /// Used to check the high availability status and current leader of Vault.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// </remarks>
        [HttpGet("leader")]
        public LeaderStatus GetLeaderStatus()
        {
            return _server.GetLeaderStatus() ?? throw new VaultServerException(
                    HttpStatusCode.ServiceUnavailable,
                    "Vault is sealed");
        }

        /// <summary>
        /// Used to unseal the Vault.
        /// </summary>
        /// <param name="requ"></param>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// 
        /// This endpoint is used to enter a single master key share to progress the unsealing
        /// of the Vault. If the threshold number of master key shares is reached, Vault will
        /// attempt to unseal the Vault. Otherwise, this API must be called multiple times
        /// until that threshold is met.
        /// <para>
        /// Either the key or reset parameter must be provided; if both are provided,
        /// reset takes precedence.
        /// </para>
        /// </remarks>
        [HttpPut("unseal")]
        public SealStatus DoUnseal([FromBody]UnsealRequest requ)
        {
            return _server.Unseal(requ.Key, requ.Reset.GetValueOrDefault())
                    // TODO:  confirm this is the correct response for this state
                    ?? throw new VaultServerException(
                            HttpStatusCode.BadRequest,
                            "server is not yet initialized");
        }


        /// <summary>
        /// Lists all enabled auth backends.
        /// </summary>
        [HttpGet("auth")]
        public ReadResponse<Dictionary<string, MountInfo>> ListAuthMounts()
        {
            _server.AssertAuthorized(this, isSudo: true);

            var mountNames = _server.ListAuthMounts().OrderBy(x => x);
            var mounts = mountNames.ToDictionary(
                    key => key.Trim('/') + "/",
                    key => new MountInfo
                    {
                        Accessor = key,
                        Type = _server.ResolveAuthMount(key).backend?.GetType().FullName ?? "(UNKNOWN)",
                        Config = new MountConfig(),
                    });
            
            // To be fully compliant with the HCVault CLI, we
            // have to honor the "repeated" response structure
            return new ReadRepeatedResponse<Dictionary<string, MountInfo>>
            {
                Data = mounts
            };
        }

        /// <summary>
        /// Lists all the mounted secret backends.
        /// </summary>
        [HttpGet("mounts")]
        public ReadResponse<Dictionary<string, MountInfo>> ListSecretMounts()
        {
            _server.AssertAuthorized(this, isSudo: true);

            var mountNames = _server.ListSecretMounts().OrderBy(x => x);
            var mounts = mountNames.ToDictionary(
                    key => key.Trim('/') + "/",
                    key => new MountInfo
                    {
                        Accessor = key,
                        Type = _server.ResolveSecretMount(key).backend?.GetType().FullName ?? "(UNKNOWN)",
                        Config = new MountConfig(),
                    });
            
            // To be fully compliant with the HCVault CLI, we
            // have to honor the "repeated" response structure
            return new ReadRepeatedResponse<Dictionary<string, MountInfo>>
            {
                Data = mounts
            };
        }

        /// <summary>
        /// Lists all configured policies.
        /// </summary>
        [HttpGet("policy")]
        public ReadResponse<PolicyKeysData> ListPolicies()
        {
            _server.AssertAuthorized(this, isSudo: true);
            
            // To be fully compliant with the HCVault CLI, we
            // have to honor the "repeated" response structure
            return new ReadRepeatedResponse<PolicyKeysData>
            {
                Data = new PolicyKeysData
                {
                    Keys = _server.ListPolicies().ToArray()
                }
            };
        }

        /// <summary>
        /// Retrieve the rules for the named policy.
        /// </summary>
        /// <param name="name">Specifies the name of the policy to retrieve.</param>
        [HttpGet("policy/{name}")]
        public ReadResponse<PolicyRulesData> ReadPolicy(
                [Required, FromRoute]string name)
        {
            _server.AssertAuthorized(this, isSudo: true);
            
            var polDef = _server.ReadPolicy(name)
                     ?? throw new VaultServerException(HttpStatusCode.NotFound);

            // To be fully compliant with the HCVault CLI, we
            // have to honor the "repeated" response structure
            return new ReadRepeatedResponse<PolicyRulesData>
            {
                Data = new PolicyRulesData
                {
                    Name = polDef.Name,
                    Rules = polDef.Definition,
                }
            };
        }

        /// <summary>
        /// Aadds a new or updates an existing policy. Once a policy is updated,
        /// it takes effect immediately to all associated users.
        /// </summary>
        /// <param name="name">Specifies the name of the policy to create.</param>
        /// <param name="policyDefinition">Specifies the policy document.</param>
        [HttpPut("policy/{name}")]
        public void WritePolicy(
                [Required, FromRoute]string name,
                [Required, FromBody]PolicyRulesData policyDefinition)
        {
            _server.AssertAuthorized(this, isSudo: true);
            
            try
            {
                _server.WritePolicy(name, policyDefinition.Rules);
            }
            catch (Exception ex)
            {
                throw DecodeServerException(ex);
            }
        }

        /// <summary>
        /// Deletes the policy with the given name. This will immediately affect
        /// all users associated with this policy.
        /// </summary>
        /// <param name="name">Specifies the name of the policy to delete.</param>
        [HttpDelete("policy/{name}")]
        public void DeletePolicy(
                [Required, FromRoute]string name)
        {
            _server.AssertAuthorized(this, isSudo: true);
            
            try
            {
                _server.DeletePolicy(name);
            }
            catch (Exception ex)
            {
                throw DecodeServerException(ex);
            }
        }

        public static Exception DecodeServerException(Exception ex)
        {
            var msgs = new string[0];
            if (!string.IsNullOrEmpty(ex.Message))
                msgs = new[] { ex.Message };

            switch (ex)
            {
                case NotSupportedException nse:
                    return new VaultServerException(HttpStatusCode.NotFound, msgs);
                case ArgumentException ae:
                    return new VaultServerException(HttpStatusCode.BadRequest, msgs);
                case System.Security.SecurityException ae:
                    return new VaultServerException(HttpStatusCode.Forbidden, msgs);
                default:
                    return new VaultServerException(HttpStatusCode.InternalServerError, msgs);
            }
        }
    }
}