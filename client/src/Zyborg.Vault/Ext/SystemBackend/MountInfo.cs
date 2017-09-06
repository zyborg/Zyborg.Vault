using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class MountInfo
    {
        [JsonProperty("accessor")]
        public string Accessor
        { get; set; }

        /// <remarks>
        /// <c>default_lease_ttl</c> or <c>max_lease_ttl</c> values of 0 mean
        /// that the system defaults are used by this backend.
        /// </remarks>
        [JsonProperty("config")]
        public MountConfig Config
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