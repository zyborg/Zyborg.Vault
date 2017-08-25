using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class SealStatus
    {
        public bool Sealed
        { get; set; }

        [JsonProperty("n")]
        public int SecretShares
        { get; set; }

        [JsonProperty("t")]
        public int SecretThreshold
        { get; set; }

        public int Progress
        { get; set; }

        public string Version
        { get; set; }

        public string Nonce
        { get; set; }
        
        [JsonProperty("cluster_name")]
        public string ClusterName
        { get; set; }

        [JsonProperty("cluster_id")]
        public string ClusterId
        { get; set; }
    }
}