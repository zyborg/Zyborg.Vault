using System.Collections.Generic;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server.Policy
{
    public class RootPolicyRule : Rule, IPolicy, IRule
    {
        Duration? IRule.MinWrappingTtl => throw new System.NotImplementedException();

        Duration? IRule.MaxWrappingTtl => throw new System.NotImplementedException();

        public override bool IsSudo => true;

        public override bool IsDenied => false;

        public override bool CheckCapability(string capability) => true;

        public override bool CheckParameters(Dictionary<string, string> parameters) => true;

        bool IPolicy.TryMatch(string path, out IRule rule)
        {
            rule = this; return true;
        }

        // public override bool CanList(Dictionary<string, string> parameters) => true;
        // public override bool CanRead(Dictionary<string, string> parameters) => true;
        // public override bool CanCreate(Dictionary<string, string> parameters) => true;
        // public override bool CanUpdate(Dictionary<string, string> parameters) => true;
        // public override bool CanDelete(Dictionary<string, string> parameters) => true;
    }
}