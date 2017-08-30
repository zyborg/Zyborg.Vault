namespace Zyborg.Vault.Protocol
{
    public interface IProtocolSource
    {
        ProtocolClient Protocol
        { get; }
    }
}