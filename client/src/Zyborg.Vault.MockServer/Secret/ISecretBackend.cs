using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Secret
{
    public interface ISecretBackend
    {
        Task<IEnumerable<string>> ListAsync(string path);

        Task<string> ReadAsync(string path);

        Task WriteAsync(string path, string payload);

        Task DeleteAsync(string path);
    }
}