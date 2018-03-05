using System.Collections.Generic;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.Token
{
    // "accessor": "d9e726d5-8f24-190d-22fa-9315c2ba60cd",
    // "creation_time": 1503609899,
    // "creation_ttl": 0,
    // "display_name": "root",
    // "expire_time": null,
    // "explicit_max_ttl": 0,
    // "id": "21bd1f5a-6eff-07fe-0184-a4358ae809c1",
    // "meta": null,
    // "num_uses": 0,
    // "orphan": true,
    // "path": "auth/token/root",
    // "policies": [
    //     "root"
    // ],
    // "ttl": 0        
    public class TokenInfo
    {
        [JsonProperty("accessor")]
        public string Accessor
        { get; set; }

        [JsonProperty("creation_time")]
        public long CreationTime
        { get; set; }

        [JsonProperty("creation_ttl")]
        public Duration CreationTtl
        { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName
        { get; set; }

        [JsonProperty("expire_time")]
        public long? ExpireTime // TODO: not sure about data type
        { get; set; }

        [JsonProperty("explicit_max_ttl")]
        public Duration MaxTtl
        { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id
        { get; set; }

        [JsonProperty("meta")]
        public Dictionary<string, string> Meta // TODO: confirm data type
        { get; set; }

        [JsonProperty("num_uses")]
        public int NumUses
        { get; set; }

        [JsonProperty("orphan")]
        public bool Orphan
        { get; set; }

        [JsonProperty("path")]
        public string Path
        { get; set; }

        [JsonProperty("policies")]
        public string[] Policies
        { get; set; }

        [JsonProperty("ttl")]
        public Duration Ttl
        { get; set; }
    }
}