using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class PluginInfo
    {
        [JsonProperty("args")]
        public string[] Args
        { get; set; }

        [JsonProperty("builtin")]
        public bool BuiltIn
        { get; set; }

        [JsonProperty("command")]
        public string Command
        { get; set; }

        [JsonProperty("name")]
        public string Name
        { get; set; }

        [JsonProperty("sha256")]
        public byte[] Sha256
        { get; set; }
    }
}