using System.Threading;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.Token
{
    public class TokenAuthOptions : CallOptions
    {
        /// <summary>
        /// Overrides the default mount path of the authentication backend being
        /// targeted.
        /// </summary>
        public string MountName
        { get; set; }
    }
}