using System;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class WrapLookupInfo
    {
        [JsonProperty("creation_path")]
        public string CreationPath
        { get; set; }

        [JsonProperty("creation_time")]
        public DateTime CreationTime
        { get; set; }

        [JsonProperty("creation_ttl")]
        public Duration CreationTtl
        { get; set; }
    }
}