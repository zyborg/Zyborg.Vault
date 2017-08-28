using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class AuthInfo
    {
        [JsonProperty("client_token")]
        public string ClientToken
        { get; set; }
        
        [JsonProperty("accessor")]
        public string Accessor
        { get; set; }
        
        [JsonProperty("policies")]
        public string[] Policies
        { get; set; }
        
        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata
        { get; set; }
        
        [JsonProperty("lease_duration")]
        public long LeaseDuration
        { get; set; }
        
        [JsonProperty("renewable")]
        public bool Renewable
        { get; set; }
        
    }
}