namespace Zyborg.Vault.Server.Policy
{
    public class PolicyDefinition
    {
        public string Name
        { get; set; }

        public string Definition
        { get; set; }

        public bool IsSystem
        { get; set; }

        public bool IsUpdateForbidden
        { get; set; }

        public bool IsDeleteForbidden
        { get; set; }

        public IPolicy Policy
        { get; set; }
    }
}