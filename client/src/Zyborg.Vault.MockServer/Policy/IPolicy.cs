using System.Collections.Generic;

namespace Zyborg.Vault.MockServer.Policy
{
    public interface IPolicy
    {
         bool TryMatch(string path, out IRule rule);
    }
}