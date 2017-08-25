using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zyborg.Vault.Internal;
using Zyborg.Vault.Model;

namespace Zyborg.Vault
{
    public class VaultClient : IDisposable
    {
        public const string DefaultVaultAddress = "http://localhost:8200";
        public const string DefaultUserAgent = "zyborg-vault-client/1.0";

        private Uri _vaultAddress;
        private TimeSpan? _timeout;
        private bool _keepAlive;
        private string _userAgent = DefaultUserAgent;
        private string _VaultToken;

        HttpClient _httpClient;

        public VaultClient(string vaultAddress = null, TimeSpan? timeout = null,
                bool keepAlive = false, string userAgent = null)
        {
            _vaultAddress = new Uri(vaultAddress ?? DefaultVaultAddress);
            _timeout = timeout;
            _keepAlive = keepAlive;
            if (userAgent != null)
                _userAgent = string.IsNullOrEmpty(userAgent) ? null : userAgent;

            Configure();
        }

        public string VaultToken
        {
            set
            {
                _VaultToken = value;
                if (_httpClient != null)
                {
                    if (!string.IsNullOrEmpty(value))
                        _httpClient.DefaultRequestHeaders.Add(Protocol.TokenHeader, value);
                    else
                        _httpClient.DefaultRequestHeaders.Remove(Protocol.TokenHeader);
                }
            }
        }

        private void Configure()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_vaultAddress, "v1/");
            _httpClient.DefaultRequestHeaders.ConnectionClose = !_keepAlive;

            if (_userAgent != null)
            {
                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            }

            if (!string.IsNullOrEmpty(_VaultToken))
            {
                _httpClient.DefaultRequestHeaders.Add(Protocol.TokenHeader, _VaultToken);
            }

            if (_timeout != null)
                _httpClient.Timeout = _timeout.Value;
        }

        public async Task<HealthStatus> GetHealthAsync()
        {
            // We want to make sure that all the valid modes or states
            // return a 200 response so we can capture it successfully
            return await SendGetAsync<HealthStatus>("sys/health"
                + "?standbyok=false"
                + "&activecode=200"
                + "&standbycode=200"
                + "&sealedcode=200"
                + "&uninitcode=200");
        }

        public async Task<SealStatus> GetSealStatusAsync()
        {
            return await SendGetAsync<SealStatus>("sys/seal-status");
        }

        public async Task<KeyStatus> GetKeyStatusAsync()
        {
            return await SendGetAsync<KeyStatus>("sys/key-status");
        }

        public async Task<InitializationStatus> GetInitializationStatusAsync()
        {
            return await SendGetAsync<InitializationStatus>("sys/init");
        }

        public async Task<InitializationResponse> DoInitializeAsync(InitializationRequest requ)
        {
            return await SendPutAsync<InitializationResponse>("sys/init", requ);
        }

        public async Task<SealStatus> DoUnsealAsync(UnsealRequest requ)
        {
            return await SendPutAsync<SealStatus>("sys/unseal", requ);
        }

        public async Task DoSealAsync()
        {
            await SendPutAsync<NoContentResponse>("sys/seal");
        }

        public async Task<LeaderStatus> GetLeaderAsync()
        {
            return await SendGetAsync<LeaderStatus>("sys/leader");
        }

        public async Task<T> SendGetAsync<T>(string uri)
        {
            using (var resp = await _httpClient.GetAsync(uri))
            {
                return await ProcessResponseAsync<T>(resp);
            }
        }

        public async Task<T> SendPutAsync<T>(string uri, object payload = null)
        {
            var payloadJson = JsonConvert.SerializeObject(payload);
            var requContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            using (var resp = await _httpClient.PutAsync(uri, requContent))
            {
                return await ProcessResponseAsync<T>(resp);
            }
        }

        private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage resp)
        {
            var respBody = await resp.Content.ReadAsStringAsync();
            var contentType = resp.Content?.Headers?.ContentType?.MediaType;
            var isJson = string.Equals("application/json", contentType);
            var isNoContent = typeof(T) == typeof(NoContentResponse);

            if (resp.IsSuccessStatusCode)
            {
                if (isNoContent)
                    return default(T);
                else if (isJson)
                    return JsonConvert.DeserializeObject<T>(respBody);
                else
                    throw new VaultClientException("unexpected content-type")
                    {
                        StatusCode = resp.StatusCode,
                        ReasonPhrase = resp.ReasonPhrase,
                        Data = {
                            ["ContentType"] = contentType,
                        }
                    };
            }
            else
            {
                var exMessage = "Response status code does not indicate success:"
                        + $" {(int)resp.StatusCode} ({resp.ReasonPhrase}).";
                var ex = new VaultClientException(exMessage)
                {
                    StatusCode = resp.StatusCode,
                    ReasonPhrase = resp.ReasonPhrase,
                    Data = {
                        ["ContentType"] = contentType,
                    }
                };
                if (isJson)
                    ex.Errors = JsonConvert.DeserializeObject<ErrorResponse>(respBody);
                else
                    ex.Data.Add("Content", respBody);

                throw ex;
            }
        }

        #region -- IDisposable Support --

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _httpClient.Dispose();
                    _httpClient = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
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

        //    private async Task<TResponse> MakeVaultApiRequest<TResponse>(string resourcePath, HttpMethod httpMethod,
        //         object requestData = null, bool rawResponse = false,
        //         Func<int, string, TResponse> customProcessor = null,
        //         string wrapTimeToLive = null) where TResponse : class
        //     {
        //         var headers = new Dictionary<string, string>();

        //         if (_lazyVaultToken != null)
        //         {
        //             headers.Add(VaultTokenHeaderKey, await _lazyVaultToken.Value);
        //         }

        //         if (wrapTimeToLive != null)
        //         {
        //             headers.Add(VaultWrapTimeToLiveHeaderKey, wrapTimeToLive);
        //         }

        //         return await _dataAccessManager.MakeRequestAsync<TResponse>(resourcePath, httpMethod, requestData, headers, rawResponse, customProcessor);
        //     }        
    }
}
