using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class PolicyRulesData
    {
        [JsonProperty("name")]
        public string Name
        { get; set; }

        [JsonProperty("rules")]
        public string Rules
        { get; set; }
    }
}