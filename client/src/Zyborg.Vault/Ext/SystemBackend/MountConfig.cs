using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class MountConfig
    {
            [JsonProperty("default_lease_ttl")]
            public Duration DefaultLeaseTtl
            { get; set; }

            [JsonProperty("force_no_cache")]
            public bool ForceNoCache
            { get; set; }

            [JsonProperty("max_lease_ttl")]
            public Duration MaxLeaseTtl
            { get; set; }
    }
}