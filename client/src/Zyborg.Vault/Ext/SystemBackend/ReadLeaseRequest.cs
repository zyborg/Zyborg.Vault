using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class ReadLeaseRequest
    {
        /// <summary>
        /// Specifies the ID of the lease to lookup.
        /// </summary>
        [JsonProperty("lease_id")]
        public string LeaseId
        { get; set; }
    }
}