using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public static class SystemBackendExtensions
    {
        public static async Task<HealthStatus> GetHealthAsync(this VaultClient client,
                SystemBackendOptions options = null)
        {
            // We want to make sure that all the valid modes or states
            // return a 200 response so we can capture it successfully
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<HealthStatus>("sys/health"
                            + "?standbyok=false"
                            + "&activecode=200"
                            + "&standbycode=200"
                            + "&sealedcode=200"
                            + "&uninitcode=200",
                            options: options);
        }

        public static async Task<InitializationStatus> GetInitializationStatusAsync(
                this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<InitializationStatus>("sys/init",
                            options: options);
        }

        public static async Task<SealStatus> GetSealStatusAsync(this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<SealStatus>("sys/seal-status",
                            options: options);
        }

        public static async Task<KeyStatus> GetKeyStatusAsync(this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<KeyStatus>("sys/key-status",
                            options: options);
        }

        public static async Task<LeaderStatus> GetLeaderAsync(this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendGetAsync<LeaderStatus>("sys/leader",
                            options: options);
        }

        public static async Task<InitializationResponse> DoInitializeAsync(this VaultClient client,
                InitializationRequest requ,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendPutAsync<InitializationResponse>("sys/init",
                            requ,
                            options: options);
        }

        public static async Task<SealStatus> DoUnsealAsync(this VaultClient client,
                UnsealRequest requ,
                SystemBackendOptions options = null)
        {
            return await ((IProtocolSource)client).Protocol
                    .SendPutAsync<SealStatus>("sys/unseal",
                            requ,
                            options: options);
        }

        public static async Task DoSealAsync(this VaultClient client,
                SystemBackendOptions options = null)
        {
            await ((IProtocolSource)client).Protocol
                    .SendPutAsync<NoContentResponse>("sys/seal",
                            options: options);
        }
    }
}