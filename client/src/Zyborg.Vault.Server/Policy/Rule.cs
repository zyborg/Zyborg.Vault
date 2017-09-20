using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server.Policy
{

    public class Rule : IRule
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


        public virtual bool IsSudo => (this.Capabilities?.Contains(Capability.Sudo)).GetValueOrDefault();

        public virtual bool IsDenied => (this.Capabilities?.Contains(Capability.Deny)).GetValueOrDefault();

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

        public virtual bool CanList(Dictionary<string, string> parameters) =>
                HasListCapability && IsAllowed(parameters);

        public virtual bool CanRead(Dictionary<string, string> parameters) =>
                HasReadCapability && IsAllowed(parameters);

        public virtual bool CanCreate(Dictionary<string, string> parameters) =>
                HasCreateCapability && IsAllowed(parameters);

        public virtual bool CanUpdate(Dictionary<string, string> parameters) =>
                HasUpdateCapability && IsAllowed(parameters);

        public virtual bool CanDelete(Dictionary<string, string> parameters) =>
                HasUpdateCapability && IsAllowed(parameters);


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
    }}