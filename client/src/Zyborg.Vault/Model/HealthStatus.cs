using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class HealthStatus
    {
        public bool Initialized
        { get; set; }

        public bool Sealed
        { get; set; }

        public bool Standby
        { get; set; }

        [JsonProperty("server_time_utc")]
        public long ServerTimeUtc
        { get; set; }

        public string Version
        { get; set; }

        [JsonProperty("cluster_name")]
        public string ClusterName
        { get; set; }

        [JsonProperty("cluster_id")]
        public string ClusterId
        { get; set; }
    }
}