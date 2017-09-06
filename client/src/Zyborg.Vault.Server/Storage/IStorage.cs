using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.Server.Storage
{
    public interface IStorage
    {
        Task<IEnumerable<string>> ListAsync(string path);

        Task<bool> ExistsAsync(string path);

        Task<string> ReadAsync(string path);

        Task WriteAsync(string path, string value);

        Task DeleteAsync(string path);
    }
}