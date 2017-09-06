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
        public Task<IEnumerable<string>> List(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Read(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task Write(string path, string payload)
        {
            throw new System.NotImplementedException();
        }

        public Task Delete(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}