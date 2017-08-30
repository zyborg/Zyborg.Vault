using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class SealStatus
    {
        public bool Sealed
        { get; set; }

        [JsonProperty("t")]
        public int SecretThreshold
        { get; set; }

        [JsonProperty("n")]
        public int SecretShares
        { get; set; }

        public int Progress
        { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Nonce
        { get; set; }

        public string Version
        { get; set; }
        
        [JsonProperty("cluster_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ClusterName
        { get; set; }

        [JsonProperty("cluster_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ClusterId
        { get; set; }
    }
}