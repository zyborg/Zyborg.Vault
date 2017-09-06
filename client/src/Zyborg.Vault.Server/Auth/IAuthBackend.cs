using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.Server.Auth
{
    public interface IAuthBackend
    {
        Task<IEnumerable<string>> List(string path);

        Task<string> Read(string path);

        Task Write(string path, string payload);

        Task Delete(string path);
    }
}