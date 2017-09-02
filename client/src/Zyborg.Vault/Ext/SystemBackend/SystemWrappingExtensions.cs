using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public static class SystemWrappingExtensions
    {
        /// <summary>
        /// Returns wrapping token properties.
        /// </summary>
        /// <param name="token">Specifies the wrapping token ID.</param>
        public static async Task<ReadResponse<WrapLookupInfo>> LookupWrappingAsync(
                this VaultClient client,
                string token,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            return await ((IProtocolSource)client).Protocol.SendPostAsync<ReadResponse<WrapLookupInfo>>(
                    "sys/wrapping/lookup",
                    new { token },
                    options: options);
        }

        /// <summary>
        /// Wraps the given values in a response-wrapped token.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data">Parameters should be supplied as keys/values in a dictionary or
        ///     custom user object. The exact set of given parameters will be contained in the
        ///     wrapped response.</param>
        /// <param name="options"></param>
        public static async Task<ReadResponse<EmptyData>> WrapDataAsync(
                this VaultClient client,
                object data,
                Duration? wrapTtl = null,
                SystemBackendOptions options = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!wrapTtl.HasValue && !(options?.WrapTtl.HasValue).HasValue)
                throw new ArgumentNullException(nameof(wrapTtl),
                        "wrapping TTL must be specified in either parameter or options property");
            if (wrapTtl.HasValue && (options?.WrapTtl.HasValue).HasValue)
                throw new ArgumentException(nameof(wrapTtl),
                        "wrapping TTL cannot be specified in both parameter and options property");
            
            CallOptions co = options;
            if (co == null)
                co = new CallOptions { WrapTtl = wrapTtl };

            return await ((IProtocolSource)client).Protocol.SendPostAsync<ReadResponse<EmptyData>>(
                    "sys/wrapping/wrap",
                    data,
                    options: co);
        }

        /// <summary>
        /// Can be used to rotate a wrapping token and refresh its TTL.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token">Specifies the wrapping token ID.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<ReadResponse<EmptyData>> RewrapDataAsync(
                this VaultClient client,
                string token,
                SystemBackendOptions options = null)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));
            
            return await ((IProtocolSource)client).Protocol.SendPostAsync<ReadResponse<EmptyData>>(
                    "sys/wrapping/rewrap",
                    new { token },
                    options: options);
        }

        /// <summary>
        /// Unwraps a wrapped response.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token">Specifies the wrapping token ID. This is required if the client
        ///     token is not the wrapping token. Do not use the wrapping token in both locations.</param>
        /// <param name="options"></param>
        /// <remarks>
        /// <para>
        /// This endpoint returns the original response inside the given wrapping token.
        /// Unlike simply reading cubbyhole/response (which is deprecated), this endpoint
        /// provides additional validation checks on the token, returns the original value
        /// on the wire rather than a JSON string representation of it, and ensures that
        /// the response is properly audit-logged.
        /// </para><para>
        /// This endpoint can be used by using a wrapping token as the client token in the
        /// API call, in which case the token parameter is not required; or, a different
        /// token with permissions to access this endpoint can make the call and pass in
        /// the wrapping token in the token parameter. Do not use the wrapping token in
        /// both locations; this will cause the wrapping token to be revoked but the value
        /// to be unable to be looked up, as it will basically be a double-use of the token!
        /// </para>
        /// </remarks>
        public static async Task<ReadResponse<T>> UnwrapData<T>(
                this VaultClient client,
                string token = null,
                SystemBackendOptions options = null)
        {
            object payload;
            if (token == null)
                payload = new {};
            else
                payload = new { token };
            
            return await ((IProtocolSource)client).Protocol.SendPostAsync<ReadResponse<T>>(
                    "sys/wrapping/unwrap",
                    payload,
                    options: options);
        }

        public static async Task<ReadResponse<Dictionary<string, object>>> UnwrapData(
                this VaultClient client,
                string token = null,
                SystemBackendOptions options = null)
        {
            return await UnwrapData<Dictionary<string, object>>(client, token, options);
        }
    }
}