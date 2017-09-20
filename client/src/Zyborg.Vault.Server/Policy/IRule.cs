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

        bool CanList(Dictionary<string, string> parameters);
        
        bool CanRead(Dictionary<string, string> parameters);
        
        bool CanCreate(Dictionary<string, string> parameters);
        
        bool CanUpdate(Dictionary<string, string> parameters);
        
        bool CanDelete(Dictionary<string, string> parameters);
    }
}