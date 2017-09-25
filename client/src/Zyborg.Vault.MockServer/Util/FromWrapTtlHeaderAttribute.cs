using Microsoft.AspNetCore.Mvc;

namespace Zyborg.Vault.MockServer.Util
{
    public class FromWrapTtlHeaderAttribute : FromHeaderAttribute
    {
        public FromWrapTtlHeaderAttribute()
        {
            base.Name = Vault.Protocol.ProtocolClient.WrapTtlHeader;
        }
    }
}