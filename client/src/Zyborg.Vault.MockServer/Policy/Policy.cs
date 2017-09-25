using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.MockServer.Policy
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
    public class Policy : IPolicy
    {
        [JsonProperty("path")]
        public Dictionary<string, Rule> PathRules
        { get; set; }

        public bool TryMatch(string path, out IRule rule)
        {
            if ((PathRules?.Count).GetValueOrDefault() > 0)
            {
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
                        if (path.StartsWith(rulePath.TrimEnd('*')))
                        {
                            rule = pr.Value;
                            return true;
                        }
                    }
                    if (string.Equals(rulePath, path))
                    {
                        rule = pr.Value;
                        return true;
                    }
                }
            }

            rule = null;
            return false;
        }
    }
}