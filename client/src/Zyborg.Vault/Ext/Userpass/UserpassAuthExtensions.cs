using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.Userpass
{
    public static class UserpassAuthExtensions
    {
        public const string AuthTypeName = "userpass";
        public const string DefaultMountName = AuthTypeName;

        /// <summary>
        /// Returns a response whose <c>Data</c> contains a Keys listing of all the users
        /// that exist in the <c>userpass</c> auth backend. <b>May return <c>null</c> if no
        /// users exist.</b>
        /// </summary>
        public static async Task<ReadResponse<KeysData>> ListUserpassUsersAsync(this VaultClient client,
                UserpassAuthOptions options = null)
        {
            var mountName = options?.MountName ?? DefaultMountName;
            return await ((IProtocolSource)client).Protocol
                    .SendListAsync<ReadResponse<KeysData>>(
                            $"auth/{mountName}/users",
                            on404: () => null,
                            //on404: () => new ReadResponse<KeysData> { Data = new KeysData { Keys = new string[0] } },
                            options: options);
        }

        public static async Task<ReadResponse<UserData>> ReadUserpassUserAsync(this VaultClient client,
                string username,
                UserpassAuthOptions options= null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            var mountName = options?.MountName ?? DefaultMountName;
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<ReadResponse<UserData>>(
                            $"auth/{mountName}/users/{username}",
                            options: options);
        }

        /// <summary>
        /// Create a new user. Honors the capabilities inside ACL policies.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="username">The username for the user.</param>
        /// <param name="password">The password for the user, required when creating the user.</param>
        /// <param name="policies">List of policies. If set to empty, only the default policy will be applicable to the user.</param>
        /// <param name="ttl">The lease duration which decides login expiration.</param>
        /// <param name="maxTtl">Maximum duration after which login should expire.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task CreateUserpassUserAsync(this VaultClient client,
                string username,
                string password,
                string[] policies = null,
                Duration? ttl = null,
                Duration? maxTtl = null,
                UserpassAuthOptions options = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Create is the same as Update, only the initial password is required
            await UpdateUserpassUserAsync(client, username,
                    password: password,
                    policies: policies,
                    ttl: ttl,
                    maxTtl: maxTtl,
                    options: options);
        }

        /// <summary>
        /// Update an existing user. 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="username">The username of the user to update.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="policies">List of policies. If set to empty, only the default policy will be applicable to the user.</param>
        /// <param name="ttl">The lease duration which decides login expiration.</param>
        /// <param name="maxTtl">Maximum duration after which login should expire.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task UpdateUserpassUserAsync(this VaultClient client,
                string username,
                string password = null,
                string[] policies = null,
                Duration? ttl = null,
                Duration? maxTtl = null,
                UserpassAuthOptions options = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            var payload = new UserData
            {
                Password = password,
                Ttl = ttl,
                MaxTtl = maxTtl,
            };

            if (policies?.Length > 0)
                payload.Policies = string.Join(",", policies);

            var mountName = options?.MountName ?? DefaultMountName;
            await ((IProtocolSource)client).Protocol
                    .SendPostAsync<NoContentResponse>(
                            $"auth/{mountName}/users/{username}",
                            payload,
                            options: options);
        }

        public static async Task DeleteUserpassUserAsync(this VaultClient client,
                string username,
                UserpassAuthOptions options = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            var mountName = options?.MountName ?? DefaultMountName;
            await ((IProtocolSource)client).Protocol
                    .SendDeleteAsync<NoContentResponse>(
                            $"auth/{mountName}/users/{username}",
                            options: options);
        }

        public static async Task UpdateUserpassUserPasswordAsync(this VaultClient client,
                string username,
                string password,
                UserpassAuthOptions options = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            var payload = new UserData { Password = password, };
            var mountName = options?.MountName ?? DefaultMountName;
            await ((IProtocolSource)client).Protocol
                    .SendPostAsync<NoContentResponse>(
                            $"auth/{mountName}/users/{username}/password",
                            payload,
                            options: options);
        }

        public static async Task UpdateUserpassUserPoliciesAsync(this VaultClient client,
                string username,
                string[] policies,
                UserpassAuthOptions options = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            var payload = new UserData
            {
                Policies = policies == null ? string.Empty : string.Join(",", policies),
            };

            var mountName = options?.MountName ?? DefaultMountName;
            await ((IProtocolSource)client).Protocol
                    .SendPostAsync<NoContentResponse>(
                            $"auth/{mountName}/users/{username}/policies",
                            payload,
                            options: options);
        }

        public static async Task<ReadResponse<EmptyData>> LoginUserpassAsync(this VaultClient client,
                string username,
                string password,
                UserpassAuthOptions options = null)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            var payload = new UserData { Password = password };
            var mountName = options?.MountName ?? DefaultMountName;
            return await ((IProtocolSource)client).Protocol
                    .SendPostAsync<ReadResponse<EmptyData>>(
                            $"auth/{mountName}/login/{username}",
                            payload,
                            options: options);
        }
    }
}