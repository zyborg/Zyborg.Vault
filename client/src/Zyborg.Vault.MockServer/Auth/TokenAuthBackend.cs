using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Zyborg.Vault.Ext.Token;
using Zyborg.Vault.MockServer.Util;
using Zyborg.Vault.MockServer.Storage;
using Microsoft.AspNetCore.Http;
using Zyborg.Vault.Model;
using Zyborg.Vault.MockServer.Controllers;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Zyborg.Vault.MockServer.Auth
{
    /// <summary>
    /// For general information about the usage and operation of the
    /// token backend, please see the
    /// <see cref="https://www.vaultproject.io/docs/auth/token.html"
    /// >Vault Token backend documentation</see>.
    /// </summary>
    /// <remarks>
    /// Often in documentation or in help channels, the "token store" is referenced.
    /// This is the same as the token authentication backend. This is a special backend
    /// in that it is responsible for creating and storing tokens, and cannot be disabled.
    /// It is also the only authentication backend that has no login capability -- all
    /// actions require existing authenticated tokens.
    /// </remarks>
    public class TokenAuthBackend : LocallyRoutedAuthBackend<TokenAuthBackend>, IAuthBackend
    {
        private MockServer _server;
        private IStorage _storage;
        private IHttpContextAccessor _httpAcc;
        private TokenManager _manager;

        public TokenAuthBackend(MockServer server, IStorage storage,
                IHttpContextAccessor httpAcc) : base(true)
        {
            _server = server;
            _storage = storage;
            _httpAcc = httpAcc;
            _manager = new TokenManager(storage);
        }

        public TokenManager Manager => _manager;

        /// <summary>
        /// This endpoint lists token accessor. This requires sudo capability,
        /// and access to it should be tightly controlled as the accessors can
        /// be used to revoke very large numbers of tokens and their associated
        /// leases at once.
        /// </summary>
        /// <returns></returns>
        [LocalListRoute("accessors")]
        public async Task<IEnumerable<string>> ListAccessors()
        {
            return await _storage.ListAsync("accessors");
        }

        public static Dictionary<string, string> ParametersFrom(object obj)
        {
            if (obj == null)
                return null;

            var jobj = JObject.FromObject(obj);
            var dict = new Dictionary<string, string>();

            foreach (var p in jobj.Properties())
            {
                if (p.HasValues)
                {
                    dict[p.Name] = p.Value.ToString();
                }
            }

            return dict;
        }

        public static Dictionary<string, string> ParametersFrom(JObject jobj)
        {
            var dict = new Dictionary<string, string>();

            foreach (var p in jobj.Properties())
            {
                if (p.HasValues)
                {
                    dict[p.Name] = p.Value.ToString();
                }
            }

            return dict;
        }

        [LocalWriteRoute("create")]
        public async Task<AuthInfo> CreateToken([Required, FromBody]string payload)
        {
            var http = _httpAcc.HttpContext;
            var jobj = JObject.Parse(payload);

            _server.AssertAuthorized(http, ParametersFrom(jobj));

            var create = jobj.ToObject<CreateParameters>();
            var state = await _manager.AddTokenAsync(create, "foo", "foo");

            return new AuthInfo
            {
                Accessor = state.Accessor,
                ClientToken = state.Id,
                LeaseDuration = state.Ttl,
                Metadata = state.Metadata,
                Policies = state.Policies,
                Renewable = state.Renewable,
            };
        }

        [LocalReadRoute("lookup")]
        public async Task<TokenInfo> LookupTokenInBody([Required, FromBody]string payload)
        {
            var tpr = JsonConvert.DeserializeObject<TokenParameterRequest>(payload);
            if (string.IsNullOrEmpty(tpr?.Token))
                throw new ArgumentException("token");
            
            return await LookupToken(tpr.Token);
        }

        [LocalReadRoute("lookup/{token}")]
        public async Task<TokenInfo> LookupToken([Required, FromRoute]string token)
        {
            var storagePath = $"tokens/{token}";
            var tokenJson = await _storage.ReadAsync(storagePath);
            if (tokenJson == null)
                throw new System.Security.SecurityException("bad token");
            
            var tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(tokenJson);
            if (tokenInfo == null)
                throw new System.Security.SecurityException("bad token");

            return await Task.FromResult(tokenInfo);
        }

        [LocalReadRoute("lookup-self")]
        public async Task<TokenInfo> LookupSelf()
        {
            var http = _httpAcc.HttpContext;
            var auth = AuthContext.From(http);
            var tokId = auth.Token;
            var state = await _manager.GetTokenAsync(tokId);

            _server.AssertAuthorized(http, null);

            var creation = (long)(state.CreateTime - MockServer.UnixEpoch).TotalSeconds;
            var expiration = (long?)null;
            if (state.ExpiresTime.HasValue)
                expiration = (long)(state.ExpiresTime.Value - MockServer.UnixEpoch).TotalSeconds;

            var info = new TokenInfo
            {
                Accessor = state.Accessor,
                CreationTime = creation,
                CreationTtl = state.Ttl ?? Duration.Zero,
                DisplayName = state.DisplayName,
                ExpireTime = expiration,
                
                Id = tokId,
                MaxTtl = state.MaxTtl ?? Duration.Zero,
                Meta = state.Metadata,
                NumUses = state.UseCount,
                Orphan = string.IsNullOrEmpty(state.ParentId),
                Path = state.CreateSource,
                Policies = state.Policies,
                Ttl = state.Ttl ?? Duration.Zero,
            };

            return info;
        }
    }
}