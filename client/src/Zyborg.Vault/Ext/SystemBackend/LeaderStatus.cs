using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    /// <summary>
    /// Represents the high availability status and current leader instance of Vault.
    /// </summary>
    public class LeaderStatus
    {
        [JsonProperty("ha_enabled")]
        public bool HaEnabled
        { get; set; }

        [JsonProperty("is_self")]
        public bool IsSelf
        { get; set; }

        [JsonProperty("leader_address")]
        public string LeaderAddress
        { get; set; }

        [JsonProperty("leader_cluster_address")]
        public string LeaderClusterAddress
        { get; set; }
    }
}