using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class UnsealRequest
    {
        /// <summary>
        /// Specifies a single master key share. This is required unless reset is true.
        /// </summary>
        public string Key
        { get; set; }

        /// <summary>
        /// Specifies if previously-provided unseal keys are discarded and the unseal process is reset.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Reset
        { get; set; }
    }
}