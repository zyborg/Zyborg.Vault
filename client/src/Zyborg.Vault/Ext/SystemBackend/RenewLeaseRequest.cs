using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class RenewLeaseRequest : ReadLeaseRequest
    {
        /// <summary>
        /// Specifies the requested amount of time (in seconds) to extend the lease.
        /// </summary>
        [JsonProperty("increment")]
        public long Increment
        { get; set; }
    }
}