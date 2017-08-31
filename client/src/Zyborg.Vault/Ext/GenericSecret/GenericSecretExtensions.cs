using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.GenericSecret
{
    public static class GenericSecretExtensions
    {
        public const string SecretTypeName = "generic";
        public const string DefaultMountName = "secret";

        /// <summary>
        /// Returns a list of key names at the specified <i>folder</i> location.
        /// </summary>
        /// <remarks>
        /// Folders are suffixed with /.
        /// The input must be a folder; list on a file will not return a value.
        /// Note that no policy-based filtering is performed on keys; do not encode sensitive information in key names.
        /// The values themselves are not accessible via this command.
        /// </remarks>
        /// <param name="path">Specifies the path of the secrets to list.</param>
        public static async Task<ReadResponse<KeysData>> ListGenericSecretsAsync(
                this VaultClient client,
                string path = null,
                GenericSecretOptions options = null)
        {
            var mountName = options?.MountName ?? DefaultMountName;
            return await client.ListAsync<ReadResponse<KeysData>>(
                    $"{mountName}/{path}",
                    on404: resp => null,
                    //on404: () => new ReadResponse<KeysData> { Data = new KeysData { Keys = new string[0] } },
                    options: options);
        }

        /// <summary>
        /// Retrieves the secret at the specified location.
        /// </summary>
        /// <remarks>
        /// Note: the lease_duration field (which on the CLI shows as refresh_interval) is advisory.
        /// No lease is created. This is a way for writers to indicate how often a given value shold
        /// be re-read by the client. See the Vault Generic backend documentation for more details.
        /// </remarks>
        /// <param name="path">Specifies the path of the secret to read.</param>
        public static async Task<ReadResponse<Dictionary<string, object>>> ReadGenericSecretAsync(
                this VaultClient client,
                string path,
                GenericSecretOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var mountName = options?.MountName ?? DefaultMountName;
            return await client.ReadAsync<ReadResponse<Dictionary<string, object>>>(
                    $"{mountName}/{path}",
                    options: options);
        }

        /// <summary>
        /// Retrieves the secret at the specified location.
        /// </summary>
        /// <remarks>
        /// Note: the lease_duration field (which on the CLI shows as refresh_interval) is advisory.
        /// No lease is created. This is a way for writers to indicate how often a given value shold
        /// be re-read by the client. See the Vault Generic backend documentation for more details.
        /// </remarks>
        /// <param name="path">Specifies the path of the secret to read.</param>
        public static async Task<ReadResponse<T>> ReadGenericSecretAsync<T>(
                this VaultClient client,
                string path,
                GenericSecretOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var mountName = options?.MountName ?? DefaultMountName;
            return await client.ReadAsync<ReadResponse<T>>(
                    $"{mountName}/{path}",
                    options: options);
        }

        /// <summary>
        /// Stores a secret at the specified location.
        /// </summary>
        /// <remarks>
        /// If the value does not yet exist, the calling token must have an ACL policy granting
        /// the create capability. If the value already exists, the calling token must have an
        /// ACL policy granting the update capability.
        /// </remarks>
        /// <param name="client"></param>
        /// <param name="path"> Specifies the path of the secrets to create/update.</param>
        /// <param name="values">Specifies key-value, pairs, to be held at the given location.
        ///     All will be returned on a read operation. A key called <c>ttl</c> will trigger
        ///     some special behavior; see the Vault Generic backend documentation for details.</param>
        /// <param name="options"></param>
        public static async Task WriteGenericSecretAsync(
                this VaultClient client,
                string path,
                Dictionary<string, object> values = null,
                // Duration? ttl = null,
                GenericSecretOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            // if (ttl.HasValue)
            //     if (values == null)
            //         values = new Dictionary<string, object> { [nameof(ttl)] = ttl.ToString() };
            //     else
            //         values[nameof(ttl)] = ttl.ToString();

            var mountName = options?.MountName ?? DefaultMountName;
            await client.WriteAsync(
                    $"{mountName}/{path}",
                    data: values,
                    options: options);
        }

        /// <summary>
        /// Stores a secret at the specified location.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"> Specifies the path of the secrets to create/update.</param>
        /// <param name="values">Specifies an object whose public properties will be stored
        ///     as key-value, pairs, to be held at the given location.
        ///     All will be returned on a read operation. A property called <c>ttl</c> will trigger
        ///     some special behavior; see the Vault Generic backend documentation for details.</param>
        /// <param name="options"></param>
        public static async Task WriteGenericSecretAsync<T>(
                this VaultClient client,
                string path,
                T values,
                GenericSecretOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var mountName = options?.MountName ?? DefaultMountName;
            await client.WriteAsync(
                    $"{mountName}/{path}",
                    data: values,
                    options: options);
        }

        /// <summary>
        /// Deletes the secret at the specified location.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path">Specifies the path of the secret to delete.</param>
        /// <param name="options"></param>
        public static async Task DeleteGenericSecretAsync(
                this VaultClient client,
                string path,
                GenericSecretOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var mountName = options?.MountName ?? DefaultMountName;
            await client.DeleteAsync(
                    $"{mountName}/{path}",
                    options: options);
        }
    }
}