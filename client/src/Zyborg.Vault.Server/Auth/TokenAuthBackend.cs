using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Zyborg.Vault.Ext.Token;
using Zyborg.Vault.Server.Protocol;
using Zyborg.Vault.Server.Storage;

namespace Zyborg.Vault.Server.Auth
{
    /// <summary>
    /// For general information about the usage and operation of the
    /// token backend, please see the
    /// <see cref="https://www.vaultproject.io/docs/auth/token.html"
    /// >Vault Token backend documentation</see>.
    /// </summary>
    public class TokenAuthBackend : LocallyRoutedAuthBackend<TokenAuthBackend>, IAuthBackend
    {
        private IStorage _storage;

        public TokenAuthBackend(IStorage storage) : base(true)
        {
            _storage = storage;
        }

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
            return await Task.FromResult((TokenInfo)null);
        }
    }
}