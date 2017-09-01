using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.SystemBackend
{
    /// <summary>
    /// Used manage secret backends in Vault.
    /// </summary>
    public static class SystemMountsExtensions
    {
        /// <summary>
        /// Lists all the mounted secret backends.
        /// </summary>
        public static async Task<ReadResponse<Dictionary<string, MountInfo>>> ListMountedBackendsAsync(
                this VaultClient client,
                SystemBackendOptions options = null)
        {
            return await client.ReadAsync<ReadResponse<Dictionary<string, MountInfo>>>(
                    "sys/mounts",
                    options: options);
        }

        /// <summary>
        /// Mounts a new secret backend at the given path.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path">Specifies the path where the secret backend will be mounted.</param>
        /// <param name="type">Specifies the type of the backend, such as <c>aws</c>.</param>
        /// <param name="description">Specifies the human-friendly description of the mount.</param>
        /// <param name="config">Specifies configuration options for this mount.</param>
        /// <param name="local">Specifies if the secret backend is a local mount only.
        ///     Local mounts are not replicated nor (if a secondary) removed by replication.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <para>
        /// This <c>config</c> an object with these possible values:
        /// <list>
        ///   <item>default_lease_ttl</item>
        ///   <item>max_lease_ttl</item>
        ///   <item>force_no_cache</item>
        ///   <item>plugin_name</item>
        /// </list>
        /// These control the default and maximum lease time-to-live, and force disabling
        /// backend caching respectively. If set on a specific mount, this overrides the
        /// global defaults.
        /// </para><para>
        /// <b><i>The <c>local</c> option is allowed in Vault open-source, but relevant functionality
        /// is only supported in Vault Enterprise.</i></b>
        /// </para>
        public static async Task MountBackendAsync(
                this VaultClient client,
                string path,
                string type,
                string description = null,
                Dictionary<string, object> config = null,
                bool? local = null,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));

            path = path.Trim('/');
            
            await ((IProtocolSource)client).Protocol.SendPostAsync<NoContentResponse>(
                    $"sys/mounts/{path}",
                    new MountBackendRequest
                    {
                        Type = type,
                        Description = description,
                        Config = config,
                        Local = local,
                    },
                    options: options);
        }

        public static async Task UnmountBackendAsync(
                this VaultClient client,
                string path,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            path = path.Trim('/');
            
            await ((IProtocolSource)client).Protocol.SendDeleteAsync<NoContentResponse>(
                    $"sys/mounts/{path}",
                    options: options);
        }

        /// <summary>
        /// Rads the given mount's configuration. Unlike the <see cref="ListMountedBackendsAsync"
        /// >mounts-listing routine</see>, this will return the current time in seconds
        /// for each TTL, which may be the system default or a mount-specific value.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path">Mount path to read configuration for.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<ReadResponse<Dictionary<string, object>>> ReadMountConfigurationAsync(
                this VaultClient client,
                string path,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            path = path.Trim('/');

            return await client.ReadAsync<ReadResponse<Dictionary<string, object>>>(
                    $"sys/mounts/{path}/tune",
                    options: options);
        }

        /// <summary>
        /// Tunes configuration parameters for a given mount point.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path">Mount path to tune configuration for.</param>
        /// <param name="config">Configuration parameters to tune.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <para>
        /// Configuration parameters:
        /// <list>
        /// <item>default_lease_ttl</item>
        /// <description>(int: 0) – Specifies the default time-to-live.
        ///     This overrides the global default.
        ///     A value of 0 is equivalent to the system default TTL.</description>
        /// <item>max_lease_ttl</item>
        /// <description>(int: 0) – Specifies the maximum time-to-live.
        ///     This overrides the global default. A value of 0 are equivalent
        ///     and set to the system max TTL.</description>
        /// </list>
        /// </para>
        /// </remarks>
        public static async Task TuneMountConfigurationAsync(
                this VaultClient client,
                string path,
                Dictionary<string, object> config,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            path = path.Trim('/');
            
            await ((IProtocolSource)client).Protocol.SendPostAsync<NoContentResponse>(
                    $"sys/mounts/{path}",
                    config,
                    options: options);
        }

        /// <summary>
        /// Remounts an already-mounted backend to a new mount point.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="from">Specifies the previous mount point.</param>
        /// <param name="to">Specifies the new destination mount point.</param>
        /// <param name="options"></param>
        public static async Task RemountBackendAsync(
                this VaultClient client,
                string from,
                string to,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(from))
                throw new ArgumentNullException(nameof(from));
            if (string.IsNullOrEmpty(to))
                throw new ArgumentNullException(nameof(to));
            
            from = from.Trim('/');
            to = to.Trim('/');

            await ((IProtocolSource)client).Protocol.SendPostAsync<NoContentResponse>(
                    $"sys/remount",
                    new { from, to },
                    options: options);
        }
    }
}