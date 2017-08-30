using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.Token
{
    public class CreateParameters
    {
        /// <summary>
        /// The ID of the client token. Can only be specified by a root token.
        /// Otherwise, the token ID is a randomly generated UUID.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id
        { get; set; }

        /// <summary>
        /// The name of the token role.
        /// </summary>
        [JsonProperty("role_name", NullValueHandling = NullValueHandling.Ignore)]
        public string RoleName
        { get; set; }

        /// <summary>
        /// A list of policies for the token. This must be a subset of the policies belonging
        /// to the token making the request, unless root. If not specified, defaults to all
        /// the policies of the calling token.
        /// </summary>
        [JsonProperty("policies", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Policies
        { get; set; }

        /// <summary>
        /// A map of string to string valued metadata. This is passed through to the audit backends.
        /// </summary>
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Meta
        { get; set; }

        /// <summary>
        /// If true and set by a root caller, the token will not have the parent token of the caller.
        /// This creates a token with no parent.
        /// </summary>
        [JsonProperty("no_parent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NoParent
        { get; set; }

        /// <summary>
        /// If true the default policy will not be contained in this token's policy set.
        /// </summary>
        [JsonProperty("no_default_policy", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NoDefaultPolicy
        { get; set; }

        /// <summary>
        /// Set to false to disable the ability of the token to be renewed past its initial TTL.
        /// Setting the value to true will allow the token to be renewable up to the system/mount
        /// maximum TTL.
        /// </summary>
        [JsonProperty("renewable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Renewable
        { get; set; }

        /// <summary>
        /// DEPRECATED; use Ttl instead
        /// </summary>
        [JsonProperty("lease", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("use Ttl instead")]
        public Duration? Lease
        { get; set; }

        /// <summary>
        /// The TTL period of the token, provided as "1h", where hour is the largest suffix.
        /// If not provided, the token is valid for the default lease TTL, or indefinitely
        /// if the root policy is used.
        /// </summary>
        [JsonProperty("ttl", NullValueHandling = NullValueHandling.Ignore)]
        public Duration? Ttl
        { get; set; }

        /// <summary>
        /// If set, the token will have an explicit max TTL set upon it. This maximum token TTL
        /// cannot be changed later, and unlike with normal tokens, updates to the system/mount
        /// max TTL value will have no effect at renewal time -- the token will never be able
        /// to be renewed or used past the value set at issue time.
        /// </summary>
        [JsonProperty("explicit_max_ttl", NullValueHandling = NullValueHandling.Ignore)]
        public Duration? ExplicitMaxTtl
        { get; set; }

        /// <summary>
        /// The display name of the token.  (Defaults to "token".)
        /// </summary>
        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName
        { get; set; }

        /// <summary>
        /// The maximum uses for the given token. This can be used to create a one-time-token
        /// or limited use token. The value of 0 has no limit to the number of uses.
        /// </summary>
        [JsonProperty("num_uses", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumUses
        { get; set; }

        /// <summary>
        /// If specified, the token will be periodic; it will have no maximum TTL
        /// (unless an "explicit-max-ttl" is also set) but every renewal will use
        /// the given period. Requires a root/sudo token to use.
        /// </summary>
        [JsonProperty("period", NullValueHandling = NullValueHandling.Ignore)]
        public Duration? Period
        { get; set; }
    }
}