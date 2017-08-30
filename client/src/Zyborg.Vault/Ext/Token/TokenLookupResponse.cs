using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.Token
{
    public class TokenLookupResponse
    {
        [JsonProperty("id")]
        public string Id
        { get; set; }

        [JsonProperty("policies")]
        public string[] Policies
        { get; set; }

        [JsonProperty("path")]
        public string Path
        { get; set; }

        [JsonProperty("meta")]
        public Dictionary<string, string> Meta
        { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName
        { get; set; }

        [JsonProperty("num_uses")]
        public int NumUses
        { get; set; }
    }
}