using System.Collections.Generic;

namespace Zyborg.Vault.Server.Policy
{
    public interface IPolicy
    {
         bool TryMatch(string path, out IRule rule);
    }
}