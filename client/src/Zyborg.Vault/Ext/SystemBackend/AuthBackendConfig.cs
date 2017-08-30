using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class AuthBackendConfig
    {
        /// <summary>
        /// Specifies the default time-to-live. If set on a specific auth path, this overrides the global default.
        /// </summary>
        [JsonProperty("default_lease_ttl", NullValueHandling = NullValueHandling.Ignore)]
        public long? DefaultLeaseTtl
        { get; set; }

        /// <summary>
        /// Specifies the maximum time-to-live. If set on a specific auth path, this overrides the global default.
        /// </summary>
        [JsonProperty("max_lease_ttl", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxLeaseTtl
        { get; set; }
    }
}