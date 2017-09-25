using Microsoft.AspNetCore.Mvc;

namespace Zyborg.Vault.MockServer.Protocol
{
    public class FromWrapTtlHeaderAttribute : FromHeaderAttribute
    {
        public FromWrapTtlHeaderAttribute()
        {
            base.Name = Vault.Protocol.ProtocolClient.WrapTtlHeader;
        }
    }
}