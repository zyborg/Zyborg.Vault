using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    /// <summary>
    /// Defines the request parameters payload when initializing a new Vault.
    /// </summary>
    public class InitializationRequest
    {
        /// <summary>
        /// Specifies the number of shares to split the master key into.
        /// </summary>
        [JsonProperty("secret_shares")]
        public int SecretShares
        { get; set; }

        /// <summary>
        /// Specifies the number of shares required to reconstruct the master key. This must be less than or equal secret_shares. If using Vault HSM with auto-unsealing, this value must be the same as secret_shares.
        /// </summary>
        [JsonProperty("secret_threshold")]
        public int SecretThreshold
        { get; set; }

        /// <summary>
        /// Specifies an array of PGP public keys used to encrypt the output unseal keys. Ordering is preserved. The keys must be base64-encoded from their original binary representation. The size of this array must be the same as secret_shares.
        /// </summary>
        [JsonProperty("pgp_keys", NullValueHandling = NullValueHandling.Ignore)]
        public string[] PgpKeys
        { get; set; }
        
        /// <summary>
        /// Specifies a PGP public key used to encrypt the initial root token. The key must be base64-encoded from its original binary representation.
        /// </summary>
        [JsonProperty("root_token_pgp_key", NullValueHandling = NullValueHandling.Ignore)]
        public string RootTokenPgpKey
        { get; set; }

        /// <summary>
        /// Specifies the number of shares that should be encrypted by the HSM and stored for auto-unsealing. Currently must be the same as secret_shares.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This option is only supported on Vault Pro/Enterprise.</i></b></para>
        /// </remarks>
        [JsonProperty("stored_shares", NullValueHandling = NullValueHandling.Ignore)]
        public int? StoredShares
        { get; set; }

        /// <summary>
        /// Specifies rhe number of shares to split the recovery key into.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This option is only supported on Vault Pro/Enterprise.</i></b></para>
        /// </remarks>
        [JsonProperty("recovery_shares", NullValueHandling = NullValueHandling.Ignore)]
        public int? RecoveryShares
        { get; set; }


        /// <summary>
        /// Specifies rhe number of shares required to reconstruct the recovery key. This must be less than or equal to recovery_shares.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This option is only supported on Vault Pro/Enterprise.</i></b></para>
        /// </remarks>
        [JsonProperty("recovery_threshold", NullValueHandling = NullValueHandling.Ignore)]
        public int? RecoveryThreshold
        { get; set; }

        /// <summary>
        /// Specifies an array of PGP public keys used to encrypt the output recovery keys. Ordering is preserved. The keys must be base64-encoded from their original binary representation. The size of this array must be the same as recovery_shares.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This option is only supported on Vault Pro/Enterprise.</i></b></para>
        /// </remarks>
        [JsonProperty("recovery_pgp_keys", NullValueHandling = NullValueHandling.Ignore)]
        public string[] RecoveryPgpKeys
        { get; set; }
    }
}