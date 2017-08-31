using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public static class SystemLeasesExtensions
    {
        /// <summary>
        /// Returns a list of lease IDs.
        /// </summary>
        /// <param name="prefix">Optional prefix to filter the list of leases.</param>
        /// <remarks>
        /// <b><i>This operation requires 'sudo' capability.</i></b>
        /// </remarks>
        public static async Task<ReadResponse<KeysData>> ListLeasesAsync(
                this VaultClient client,
                string prefix = null,
                SystemBackendOptions options = null)
        {
            var path = "sys/leases/lookup/";
            if (!string.IsNullOrEmpty(prefix))
                path += $"{prefix.TrimStart('/')}";

            return await client.ListAsync<ReadResponse<KeysData>>(path,
                    on404: resp => null,
                    options: options);
        }

        /// <summary>
        /// Retrieve lease metadata.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="leaseId">Specifies the ID of the lease to lookup.</param>
        /// <param name="options"></param>
        public static async Task<LeaseInfo> ReadLeaseAsync(
                this VaultClient client,
                string leaseId,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            return await ((IProtocolSource)client).Protocol.SendPutAsync<LeaseInfo>(
                    "sys/leases/lookup",
                    new ReadLeaseRequest { LeaseId = leaseId },
                    options: options);
        }

        /// <summary>
        /// Renews a lease, requesting to extend the lease.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="leaseId">Specifies the ID of the lease to extend.</param>
        /// <param name="increment">Specifies the requested amount of time (in seconds) to extend the lease.</param>
        /// <param name="options"></param>
        public static async Task<LeaseInfo> RenewLeaseAsync(
                this VaultClient client,
                string leaseId,
                long increment = 0,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            return await ((IProtocolSource)client).Protocol.SendPutAsync<LeaseInfo>(
                    "sys/leases/renew",
                    new RenewLeaseRequest
                    {
                        LeaseId = leaseId,
                        Increment = increment,
                    },
                    options: options);
        }

        /// <summary>
        /// Revokes a lease immediately.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="leaseId">Specifies the ID of the lease to revoke.</param>
        /// <param name="options"></param>
        public static async Task RevokeLeaseAsync(
                this VaultClient client,
                string leaseId,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            await ((IProtocolSource)client).Protocol.SendPutAsync<NoContentResponse>(
                    "sys/leases/revoke",
                    new ReadLeaseRequest { LeaseId = leaseId, },
                    options: options);
            
        }

        /// <summary>
        /// Revokes all secrets (via a lease ID prefix) or tokens (via the tokens' path property)
        /// generated under a given prefix immediately. This requires sudo capability and access
        /// to it should be tightly controlled as it can be used to revoke very large numbers of
        /// secrets/tokens at once.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="leaseId">Specifies the prefix to revoke.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <b><i>This operation requires 'sudo' capability.</i></b>
        /// </remarks>
        public static async Task RevokePrefixLeasesAsync(
                this VaultClient client,
                string prefix,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentNullException(nameof(prefix));

            await ((IProtocolSource)client).Protocol.SendPutAsync<NoContentResponse>(
                    $"sys/leases/revoke-prefix/{prefix}",
                    options: options);
        }

        /// <summary>
        /// This endpoint revokes all secrets or tokens generated under a given prefix
        /// immediately. Unlike /sys/leases/revoke-prefix, this path ignores backend errors
        /// encountered during revocation. This is potentially very dangerous and should only
        /// be used in specific emergency situations where errors in the backend or the
        /// connected backend service prevent normal revocation.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="leaseId">Specifies the prefix to revoke.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <b><i>This operation requires 'sudo' capability.</i></b>
        /// <para>
        /// By ignoring these errors, Vault abdicates responsibility for ensuring that
        /// the issued credentials or secrets are properly revoked and/or cleaned up.
        /// Access to this operation is typically tightly controlled.
        /// </para>
        /// </remarks>
        public static async Task RevokeForceLeasesAsync(
                this VaultClient client,
                string prefix,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentNullException(nameof(prefix));

            await ((IProtocolSource)client).Protocol.SendPutAsync<NoContentResponse>(
                    $"sys/leases/revoke-force/{prefix}",
                    options: options);
        }
    }
}