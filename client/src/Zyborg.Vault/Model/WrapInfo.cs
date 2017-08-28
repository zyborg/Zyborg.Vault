using System;
using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class WrapInfo
    {
        [JsonProperty("token")]
        public string Token
        { get; set; }

        [JsonProperty("ttl")]
        public long TTL
        { get; set; }

        [JsonProperty("creation_time")]
        public DateTime CreationTime
        { get; set; }

        [JsonProperty("creation_path")]
        public string CreationPath
        { get; set; }
    }
}