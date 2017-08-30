using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.Userpass
{
    public class UserData
    {
        /// <summary>
        /// The password for the user. Only required when creating the user.
        /// </summary>
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password
        { get; set; }

        /// <summary>
        /// List of policies. If empty, only the default policy will be applicable to the user.
        /// </summary>
        [JsonProperty("policies", NullValueHandling = NullValueHandling.Ignore)]
        public string Policies
        { get; set; }

        /// <summary>
        /// The lease duration which decides login expiration.
        /// </summary>
        [JsonProperty("ttl", NullValueHandling = NullValueHandling.Ignore)]
        public Duration? Ttl
        { get; set; }

        /// <summary>
        /// Maximum duration after which login should expire.
        /// </summary>
        [JsonProperty("max_ttl", NullValueHandling = NullValueHandling.Ignore)]
        public Duration? MaxTtl
        { get; set; }
    }
}