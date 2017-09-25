using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Auth
{
    public interface IAuthBackend
    {
        Task<IEnumerable<string>> ListAsync(string path);

        Task<object> ReadAsync(string path);

        Task<object> WriteAsync(string path, string payload);

        Task DeleteAsync(string path);
    }
}