using System.Threading;

namespace Zyborg.Vault.Model
{
    public class CallOptions
    {

        /// <summary>
        /// Specifies that a response should be <i>wrapped</i> instead of
        /// returned directly, and a response-wrapped token with the given
        /// TTL lifetime be returned instead.
        /// </summary>
        /// <remarks>
        /// See the <see cref="https://www.vaultproject.io/docs/secrets/cubbyhole/index.html#response-wrapping"
        /// >Vault documentation</see> for more details.
        /// </remarks>
        public virtual Duration? WrapTtl
        { get; set; }

        /// <summary>
        /// Provides a Cancellation Token that can be used to cancel an
        /// asynchronous operation.  Only valid for <c>async</c> calls,
        /// ignored otherwise.
        /// </summary>
        /// <returns></returns>
        public virtual CancellationToken? Cancel
        { get; set; }
    }
}