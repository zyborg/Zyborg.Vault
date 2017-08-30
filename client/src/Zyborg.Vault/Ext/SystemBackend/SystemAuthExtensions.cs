using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public static class SystemAuthExtensions
    {
        public static async Task<ReadResponse<Dictionary<string, AuthBackendInfo>>> ListAuthBackendsAsync(
                this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<ReadResponse<Dictionary<string, AuthBackendInfo>>>($"sys/auth",
                            options: options);
        }

        public static async Task<AuthBackendConfig> ReadAuthBackendTuningAsync(
                this VaultClient client,
                string authName,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(authName))
                throw new ArgumentNullException(nameof(authName));
                
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<AuthBackendConfig>($"sys/auth/{authName}/tune",
                            options: options);
        }

        public static async Task TuneAuthBackendAsync(this VaultClient client,
                string authName,
                AuthBackendConfig config,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(authName))
                throw new ArgumentNullException(nameof(authName));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
                
            await ((IProtocolSource)client).Protocol
                    .SendPostAsync<NoContentResponse>($"sys/auth/{authName}/tune",
                            config,
                            options: options);
        }
    }
}