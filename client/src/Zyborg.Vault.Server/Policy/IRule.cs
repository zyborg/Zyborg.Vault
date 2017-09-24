using System.Collections.Generic;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server.Policy
{
    public interface IRule
    {
        Duration? MinWrappingTtl
        { get; }

        Duration? MaxWrappingTtl
        { get; }

        bool IsSudo
        { get; }

        bool IsDenied
        { get; }

        /// <summary>
        /// Tests whether the argument capability is contained within the rule.
        /// NOTE:  this does not, necessarily indicate if the capability is
        /// allowed by the rule, as it may also be explicitly <i>denied</i> or
        /// may have a further requiremenht for <i>sudo</i> privilege which
        /// must be tested seperately.
        /// </summary>
        bool CheckCapability(string capability);

        /// <summary>
        /// Tests whether the given set of parameters are allowed as per
        /// the parameter restrictions (if any) contained within the rule.
        /// </summary>
        bool CheckParameters(Dictionary<string, string> parameters);

        // bool CanList(Dictionary<string, string> parameters);
        
        // bool CanRead(Dictionary<string, string> parameters);
        
        // bool CanCreate(Dictionary<string, string> parameters);
        
        // bool CanUpdate(Dictionary<string, string> parameters);
        
        // bool CanDelete(Dictionary<string, string> parameters);
    }
}