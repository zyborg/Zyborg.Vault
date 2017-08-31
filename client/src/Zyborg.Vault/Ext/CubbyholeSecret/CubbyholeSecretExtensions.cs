using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.CubbyholeSecret
{
    public static class CubbyholeSecretExtensions
    {
        public const string SecretTypeName = "cubbyhole";
        public const string MountName = SecretTypeName;

        public static async Task<ReadResponse<KeysData>> ListCubbyholeSecretsAsync(
                this VaultClient client,
                string path = null,
                CallOptions options = null)
        {
            return await client.ListAsync<ReadResponse<KeysData>>(
                    $"{MountName}/{path}",
                    on404: resp => null,
                    //on404: () => new ReadResponse<KeysData> { Data = new KeysData { Keys = new string[0] } },
                    options: options);
        }

        public static async Task<ReadResponse<Dictionary<string, object>>> ReadCubbyholeSecretAsync(
                this VaultClient client,
                string path,
                CallOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return await client.ReadAsync<ReadResponse<Dictionary<string, object>>>(
                    $"{MountName}/{path}",
                    options: options);
        }

        public static async Task<ReadResponse<T>> ReadCubbyholeSecretAsync<T>(
                this VaultClient client,
                string path,
                CallOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return await client.ReadAsync<ReadResponse<T>>(
                    $"{MountName}/{path}",
                    options: options);
        }

        public static async Task WriteCubbyholeSecretAsync(
                this VaultClient client,
                string path,
                Dictionary<string, object> values = null,
                CallOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            await client.WriteAsync(
                    $"{MountName}/{path}",
                    data: values,
                    options: options);
        }

        public static async Task WriteCubbyholeSecretAsync<T>(
                this VaultClient client,
                string path,
                T values,
                CallOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            await client.WriteAsync(
                    $"{MountName}/{path}",
                    data: values,
                    options: options);
        }

        public static async Task DeleteCubbyholeSecretAsync(
                this VaultClient client,
                string path,
                CallOptions options = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            await client.DeleteAsync(
                    $"{MountName}/{path}",
                    options: options);
        }
    }
}