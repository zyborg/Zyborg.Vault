using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zyborg.Vault.Ext.Token;
using Zyborg.Vault.MockServer.Storage;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.MockServer.Auth
{
    public class TokenManager
    {
        private IStorage _storage;

        public TokenManager(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var path = $"tokens/{HashTokenId(id)}";
            return await _storage.ExistsAsync(path);
        }

        public async Task<TokenState> GetTokenAsync(string id)
        {
            var path = $"tokens/{HashTokenId(id)}";
            var json = await _storage.ReadAsync(path);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<TokenState>(json);
        }
        
        public async Task<TokenState> GetTokenByAccessorAsync(string acc)
        {
            var path = $"accessors/{acc}";
            var id = await _storage.ReadAsync(path);
            return await GetTokenAsync(id);
        }

        public async Task<bool> RemoveTokenAsync(string id)
        {
            var tokPath = $"tokens/{HashTokenId(id)}";
            var json = await _storage.ReadAsync(tokPath);

            if (string.IsNullOrEmpty(json))
                return false;

            var state = JsonConvert.DeserializeObject<TokenState>(json);
            var accPath = $"accessors/{state.Accessor}";
            await _storage.DeleteAsync(accPath);
            await _storage.DeleteAsync(tokPath);
            return true;
        }

        public async Task<TokenState> AddTokenAsync(CreateParameters tokenParams,
                string createSource, string parentId)
        {
            var tokenId = tokenParams.Id ?? Guid.NewGuid().ToString();
            if (await ExistsAsync(tokenId))
                // Ultimately should generate a 400 Bad Request
                throw new ArgumentException("cannot create a token with a duplicate ID");
            
#pragma warning disable 618
            // For backward-compatibility, we also fallback
            // to the old (and obsolete) "Lease" parameter
            var ttl = tokenParams.Ttl ?? tokenParams.Lease;
#pragma warning restore 618
            var max = tokenParams.ExplicitMaxTtl;
            var now = DateTime.UtcNow;

            // TODO: also need to incorporate global limits in this computation
            DateTime? exp = null;
            if (ttl.HasValue)
            {
                if (max.HasValue && max.Value < ttl.Value)
                    exp = now + (TimeSpan)max;
                else
                    exp = now + (TimeSpan)ttl;
            }
            else if (max.HasValue)
            {
                exp = now + (TimeSpan)max;
            }

            var state = new TokenState
            {
                CreateTime = now,
                ExpiresTime = exp,
                CreateSource = createSource,

                Id = tokenId,
                Accessor = Guid.NewGuid().ToString(),
                ParentId = parentId,

                DisplayName = tokenParams.DisplayName,
                MaxTtl = max,
                Policies = tokenParams.Policies,
                Renewable = tokenParams.Renewable.GetValueOrDefault(true),
                RenewalPeriod = tokenParams.Period,
                Role = tokenParams.RoleName,
                Ttl = ttl,
                UseLimit = tokenParams.NumUses,
                UseCount = 0,
            };

            var hash = HashTokenId(state.Id);
            var path = $"tokens/{hash}";
            var json = JsonConvert.SerializeObject(state);
            await _storage.WriteAsync(path, json);

            path = $"accessors/{state.Accessor}";
            await _storage.WriteAsync(path, state.Id);

            return state;
        }

        public string HashTokenId(string id)
        {
            // TODO: compute a good hash of ID such
            // as BCrypt or SCrypt for a lookup key
            var hash = new string(id.Reverse().ToArray());

            return hash;
        }
    }
}