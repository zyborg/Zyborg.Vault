using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.Server.Secret
{
    /// <summary>
    /// A placeholder Backend that does nothing but throws <c>NotImplementedException</c>
    /// for each implemented method.
    /// </summary>
    public class DummyBackend : ISecretBackend
    {
        public Task<IEnumerable<string>> ListAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ReadAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAsync(string path, string payload)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}