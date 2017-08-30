using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class AuthBackendInfo
    {
        [JsonProperty("accessor")]
        public string Accessor
        { get; set; }

        [JsonProperty("config")]
        public AuthBackendConfig Config
        { get; set; }

        [JsonProperty("description")]
        public string Description
        { get; set; }

        [JsonProperty("local")]
        public bool Local
        { get; set; }

        [JsonProperty("type")]
        public string Type
        { get; set; }
    }
}