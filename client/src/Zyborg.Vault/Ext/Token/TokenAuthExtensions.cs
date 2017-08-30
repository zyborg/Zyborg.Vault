using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.Token
{
    public static class TokenAuthExtensions
    {
        public const string AuthTypeName = "token";
        public const string DefaultMountName = AuthTypeName;

        /// <summary>
        /// Lists token accessor. This requires sudo capability, and access to it should be tightly controlled as the accessors can be used to revoke very large numbers of tokens and their associated leases at once.
        /// </summary>
        /// <returns></returns>
        public static async Task<ReadResponse<KeysData>> ListTokenAccessorsAsync(this VaultClient client,
                TokenAuthOptions options = null)
        {
            var mountName = options?.MountName ?? DefaultMountName;
            return await ((IProtocolSource)client).Protocol
                    .SendListAsync<ReadResponse<KeysData>>(
                            $"auth/{mountName}/accessors",
                            options: options);
        }

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <remarks>
        /// Certain options are only available when called by a root token.
        public static async Task<ReadResponse<EmptyData>> CreateTokenAsync(this VaultClient client,
                CreateParameters createParameters = null,
                TokenAuthOptions options = null)
        {
            return await CreateTokenAsync(client, false, null, createParameters,
                    options: options);
        }

        /// <summary>
        /// Creates a new orphan token.
        /// A root token is not required to create an orphan token
        /// (otherwise set with the no_parent option).
        /// </summary>
        /// <remarks>
        /// Certain options are only available when called by a root token.
        /// </remarks>
        public static async Task<ReadResponse<EmptyData>> CreateOrphanTokenAsync(this VaultClient client,
                CreateParameters createParameters = null,
                TokenAuthOptions options = null)
        {
            return await CreateTokenAsync(client, true, null, createParameters,
                    options: options);
        }

        /// <summary>
        /// Creates a new token against the specified role name.
        /// This may override options set during the call.
        /// </summary>
        /// <remarks>
        /// Certain options are only available when called by a root token.
        /// </remarks>
        public static async Task<ReadResponse<EmptyData>> CreateRoleTokenAsync(this VaultClient client,
                string roleName,
                CreateParameters createParameters = null,
                TokenAuthOptions options = null)
        {
            return await CreateTokenAsync(client, false, roleName, createParameters,
                    options: options);
        }

        private static async Task<ReadResponse<EmptyData>> CreateTokenAsync(VaultClient client,
                bool createOrphan, string roleName,
                CreateParameters createParameters = null,
                TokenAuthOptions options = null)
        {
            var mountName = options?.MountName ?? DefaultMountName;
            return await ((IProtocolSource)client).Protocol
                    .SendPostAsync<ReadResponse<EmptyData>>(
                            $"auth/{mountName}/create",
                            createParameters,
                            options: options);
        }
    }
}