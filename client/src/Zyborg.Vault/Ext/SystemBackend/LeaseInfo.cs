using System;
using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class LeaseInfo
    {
        [JsonProperty("id")]
        public string Id
        { get; set; }

        [JsonProperty("issue_time")]
        public DateTime? IssueTime
        { get; set; }

        [JsonProperty("expire_time")]
        public DateTime? ExpireTime
        { get; set; }

        [JsonProperty("last_renewal_time")]
        public DateTime? LastRenewalTime
        { get; set; }

        [JsonProperty("renewable")]
        public bool Renewable
        { get; set; }

        [JsonProperty("ttl")]
        public long? Ttl
        { get; set; }
    }
}