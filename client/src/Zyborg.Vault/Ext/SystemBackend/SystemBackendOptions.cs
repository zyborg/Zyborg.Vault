using System;
using System.Threading;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class SystemBackendOptions : CallOptions
    {
        /// <summary>
        /// Overridden to prevent setting.  Response-wrapping is not supported for the system backend.
        /// </summary>
        /// <remarks>
        /// See the <see cref="https://www.vaultproject.io/docs/secrets/cubbyhole/index.html#response-wrapping"
        /// >Vault documentation</see> for more details.
        /// </remarks>
        public override Duration? WrapTtl
        {
            get => null;
            set => throw new InvalidOperationException(
                    "system backend does not support response-wrapping");
        }
    }
}