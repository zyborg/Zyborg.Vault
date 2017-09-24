using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Zyborg.Vault.Server.Policy
{
    public static class PolicyManager
    {
        public const string RootPolicy = "root";
        public const string DefaultPolicy = "default";

        public static readonly IPolicy[] NoPolicies = new IPolicy[0];

        public static string GetDefaultDefinition()
        {
            using (var s = typeof(PolicyManager).Assembly.GetManifestResourceStream(
                    typeof(PolicyManager), "default-policy-manual.json"))
            using (var r = new StreamReader(s))
            {
                return r.ReadToEnd();
            }
        }

        public static Policy ParseDefinition(string def)
        {
            return JsonConvert.DeserializeObject<Policy>(def);
        }

        public static Dictionary<string, PolicyDefinition> GetBasePolicies()
        {
            var defPol = GetDefaultDefinition();
            return new Dictionary<string, PolicyDefinition>
            {
                [RootPolicy] = new PolicyDefinition
                {
                    Name = RootPolicy,
                    Definition = string.Empty,
                    Policy = new RootPolicyRule(),
                    IsSystem = true,
                    IsDeleteForbidden = true,
                    IsUpdateForbidden = true,
                },
                [DefaultPolicy] = new PolicyDefinition
                {
                    Name = DefaultPolicy,
                    Definition = defPol,
                    Policy = ParseDefinition(defPol),
                    IsSystem = true,
                    IsDeleteForbidden = true,
                    // Update IS allowed
                },
            };
        }

        public static bool IsAuthorized(string capability, string path,
                Dictionary<string, string> parameters, bool isSudo = false,
                params IPolicy[] policies)
        {
            bool hasParams = parameters == null || parameters.Count == 0;
            bool hasSudo = false;
            bool hasCap = false;

            foreach (var p in policies)
            {
                if (!p.TryMatch(path, out var rule))
                    continue;

                // We can abort the whole thing if ever find a denial
                if (rule.IsDenied)
                    return false;

                // If we haven't already found a component of the authorization
                // in any matching rule, test to see if it's there
                if (!hasCap)
                    hasCap = rule.CheckCapability(capability);
                if (!hasParams)
                    hasParams = rule.CheckParameters(parameters);
                if (!hasSudo)
                    hasSudo = rule.IsSudo;
            }

            // Authorized if all components pass
            return (!isSudo || hasSudo) && hasCap && hasParams;
        }
    }
}