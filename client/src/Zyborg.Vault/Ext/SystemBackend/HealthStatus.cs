using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class HealthStatus
    {
        public virtual bool Initialized
        { get; set; }

        public virtual bool Sealed
        { get; set; }

        public virtual bool Standby
        { get; set; }

        [JsonProperty("server_time_utc")]
        public virtual long ServerTimeUtc
        { get; set; }

        public virtual string Version
        { get; set; }

        [JsonProperty("cluster_name", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string ClusterName
        { get; set; }

        [JsonProperty("cluster_id", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string ClusterId
        { get; set; }
    }
}