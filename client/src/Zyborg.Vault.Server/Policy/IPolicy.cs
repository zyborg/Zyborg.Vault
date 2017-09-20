namespace Zyborg.Vault.Server.Policy
{
    public interface IPolicy
    {
         IRule TryMatch(string path);
    }
}