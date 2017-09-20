using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server.Policy
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Interesting implementation detail from:  https://www.vaultproject.io/docs/concepts/policies.html#tokens :
    /// <quote>
    /// There is no way to modify the policies associated with a token once the token has been
    /// issued. The token must be revoked and a new one acquired to receive a new set of policies.
    /// 
    /// However, the contents of policies are parsed in real-time at every token use. As a result,
    /// if a policy is modified, the modified rules will be in force the next time a token with
    /// that policy attached is used to make a call to Vault.
    /// </quote>
    /// To make this performant, caching of policy evaluation should be considered:
    /// * Digest Policy content and remember the hash
    /// * Incoming Token is evaluated against policy and then cache result with the hash
    /// * Subsequent, check if policy hash has changed, (or policy list has changed) and
    ///   if not, reuse previous evaluation result
    /// </para>
    /// </remarks>
    public class Policy
    {
        [JsonProperty("path")]
        public Dictionary<string, Rule> PathRules
        { get; set; }

        public Rule TryMatch(string path)
        {
            if ((PathRules?.Count).GetValueOrDefault() == 0)
                return null;
            
            // Based on:
            //    https://www.vaultproject.io/docs/concepts/policies.html#policy-syntax
            // 1. Policy paths are matched using the most specific path match. This may be an
            //    exact match or the longest-prefix match of a glob. This means if you define
            //    a policy for "secret/foo*", the policy would also match "secret/foobar".
            // 2. The glob character is only supported as the last character of the path, and
            //    is not a regular expression!

            // We do reverse sorting to find the longest match
            foreach (var pr in PathRules.OrderByDescending(x => x.Key.Length))
            {
                var rulePath = pr.Key;
                if (rulePath.EndsWith("*"))
                {
                    rulePath.TrimEnd('*');
                    if (path.StartsWith(rulePath.TrimEnd('*')))
                        return pr.Value;
                }
                if (string.Equals(rulePath, path))
                    return pr.Value;
            }

            // No rule paths match
            return null;
        }
    }

    public class Rule
    {
        public const string WildcardParmeter = "*";
        public const char WildcardGlob = '*';

        [JsonProperty("capabilities")]
        public IEnumerable<string> Capabilities
        { get; set; }

        [JsonProperty("allowed_parameters")]
        public Dictionary<string, IEnumerable<string>> AllowedParameters
        { get; set; }

        [JsonProperty("denied_parameters")]
        public Dictionary<string, IEnumerable<string>> DeniedParameters
        { get; set; }

        [JsonProperty("min_wrapping_ttl")]
        public Duration? MinWrappingTtl
        { get; set; }

        [JsonProperty("max_wrapping_ttl")]
        public Duration? MaxWrappingTtl
        { get; set; }


        public bool IsSudo => (this.Capabilities?.Contains(Capability.Sudo)).GetValueOrDefault();
        public bool IsDenied => (this.Capabilities?.Contains(Capability.Deny)).GetValueOrDefault();

        public bool HasListCapability => !IsDenied &&
                (this.Capabilities?.Contains(Capability.List)).GetValueOrDefault();
        public bool HasReadCapability => !IsDenied &&
                (this.Capabilities?.Contains(Capability.Read)).GetValueOrDefault();
        public bool HasCreateCapability => !IsDenied &&
                (this.Capabilities?.Contains(Capability.Create)).GetValueOrDefault();
        public bool HasUpdateCapability => !IsDenied &&
                (this.Capabilities?.Contains(Capability.Update)).GetValueOrDefault();
        public bool HasDeleteCapability => !IsDenied &&
                (this.Capabilities?.Contains(Capability.Delete)).GetValueOrDefault();

        public bool CanList(string op, Dictionary<string, string> parameters) =>
                HasListCapability && IsAllowed(parameters);

        public bool IsAllowed(Dictionary<string, string> parameters)
        {
            if ((parameters?.Count).GetValueOrDefault() == 0)
                // No parameters provided, so nothing to test against
                return true;

            bool allowedParams = (AllowedParameters?.Count).GetValueOrDefault() > 0;
            bool deniedParams = (DeniedParameters?.Count).GetValueOrDefault() > 0;

            if (!allowedParams && !deniedParams)
                // No restrictions on parameters
                return true;

            foreach (var p in parameters)
            {
                if (deniedParams)
                    if (IsParameterMatch(p.Key, p.Value, DeniedParameters))
                        return false;

                if (allowedParams)
                    if (IsParameterMatch(p.Key, p.Value, DeniedParameters))
                        return true;
                    else
                        return false;
            }

            // This should actually never hit
            // because of the earlier checks
            return true;
        }

        /// <summary>
        /// Tests if a parameter name + value matches against as set of patterns.
        /// </summary>
        /// <remarks>
        /// The rules for parameter patterns and matching are gleaned from:
        ///    https://www.vaultproject.io/docs/concepts/policies.html#allowed-and-denied-parameters
        /// and obide by the following semantics:
        /// <list>
        /// <item>A pattern can match by exact name</item>
        /// <item>A pattern can match by wildcard <c>*</c></item>
        /// <item>A pattern can allow any value (missing or empty value list)</item>
        /// <item>A pattern can allow specific values (a non-tempy list)</item>
        /// <item>A pattern can allow value globs (values with prefix and/or suffix wildcard <c>*</c>)</item>
        /// </list>
        /// </remarks>
        public bool IsParameterMatch(string name, string value,
                Dictionary<string, IEnumerable<string>> patterns)
        {
            if (patterns.TryGetValue(name, out var patternValues)
                    || patterns.TryGetValue(WildcardParmeter, out patternValues))
            {
                // Test for either *any* value or an exact-match value     
                if ((patternValues?.Count()).GetValueOrDefault() == 0
                        || patternValues.Contains(value))
                    return true;
                        
                // We also have to test for value glob patterns
                foreach (var dv in patternValues)
                {
                    if (dv.StartsWith(WildcardGlob))
                    {
                        if (dv.EndsWith(WildcardGlob))
                        {
                            // *XXX*
                            if (value.Contains(dv.Trim(WildcardGlob)))
                                return true;
                        }
                        else
                        {
                            // *XXX
                            if (value.EndsWith(dv.TrimStart(WildcardGlob)))
                                return true;
                        }
                    }
                    else if (dv.EndsWith(WildcardGlob))
                    {
                        // XXX*
                        if (value.StartsWith(dv.TrimEnd(WildcardGlob)))
                            return true;
                    }
                }
            }

            return false;
        }
    }

    public static class Capability
    {
        public const string List = "list";
        public const string Read = "read";
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Sudo = "sudo";
        public const string Deny = "deny";

        public static readonly IEnumerable<string> All = new[]
        {
            List, Read, Create, Update, Delete, Sudo, Deny
        };

        public static readonly IEnumerable<string> AllAllow = new[]
        {
            List, Read, Create, Update, Delete, Sudo
        };

        public static readonly IEnumerable<string> AllDeny = new[]
        {
            Deny
        };
    }
}