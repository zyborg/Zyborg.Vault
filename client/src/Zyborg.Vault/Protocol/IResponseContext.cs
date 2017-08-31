using System.Net.Http.Headers;

namespace Zyborg.Vault.Protocol
{
    public interface IResponseContext
    {
        bool IsJsonBody
        { get; }

        string Body
        { get; }

        // HttpHeaders Headers
        // { get; }
    }
}