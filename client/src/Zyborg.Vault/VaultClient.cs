using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault
{
    /// <summary>
    /// Implements a higher-level client that provides the foundational,
    /// logical operations of the Vault service.
    /// </summary>
    public class VaultClient : IProtocolSource, IDisposable
    {
        private ProtocolClient _Protocol;

        public VaultClient(string vaultAddress = null, TimeSpan? timeout = null,
                bool keepAlive = false, string userAgent = null)
        {
            _Protocol = new ProtocolClient(vaultAddress, timeout, keepAlive, userAgent);
        }

        public string VaultToken
        {
            set => _Protocol.VaultToken = value;
        }

        ProtocolClient IProtocolSource.Protocol => _Protocol;

        public async Task<HelpResponse> GetHelpAsync(string path,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));
            if (path.Contains("?"))
                path += "&help=1";
            else
                path += "?help=1";

            return await _Protocol.SendGetAsync<HelpResponse>(path, options);
        }

        public async Task<T> ListAsync<T>(string path,
                Func<T> on404 = null,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));

            return await _Protocol.SendListAsync<T>(path, on404: on404, options: options);
        }

        public async Task<T> ReadAsync<T>(string path,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));

            return await _Protocol.SendGetAsync<T>(path, options);
        }

        public async Task WriteAsync(string path, object data,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));

            await _Protocol.SendPutAsync<NoContentResponse>(path, data, options);
        }

        public async Task<T> WriteAsync<T>(string path, object data,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));

            return await _Protocol.SendPutAsync<T>(path, data, options);
        }

        public async Task DeleteAsync(string path,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));

            await _Protocol.SendDeleteAsync<NoContentResponse>(path, options);            
        }

        public async Task DeleteAsync<T>(string path,
                CallOptions options = null)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path must be relative", nameof(path));

            await _Protocol.SendDeleteAsync<T>(path, options);            
        }


        #region -- IDisposable Support --

        /// To detect redundant calls
        public bool IsDisposed
        { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _Protocol.Dispose();
                    _Protocol = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VaultClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion -- IDisposable Support --
    }
}
