using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Protocol
{
    /// <summary>
    /// Implements the lower-level protocol at the HTTP level of the Vault service.
    /// </summary>
    public class ProtocolClient : IDisposable
    {
        public const string TokenHeader = "X-Vault-Token";
        public const string WrapTtlHeader = "X-Vault-Wrap-TTL";

        public const string DefaultVaultAddress = "http://localhost:8200";
        public const string DefaultUserAgent = "zyborg-vault-client/1.0";

        public static readonly HttpMethod ListHttpMethod = new HttpMethod("LIST");

        private Uri _vaultAddress;
        private TimeSpan? _timeout;
        private bool _keepAlive;
        private string _userAgent = DefaultUserAgent;
        private string _VaultToken;

        HttpClient _httpClient;


        public ProtocolClient(string vaultAddress = null, TimeSpan? timeout = null,
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
                        _httpClient.DefaultRequestHeaders.Add(TokenHeader, value);
                    else
                        _httpClient.DefaultRequestHeaders.Remove(TokenHeader);
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
                _httpClient.DefaultRequestHeaders.Add(TokenHeader, _VaultToken);
            }

            if (_timeout != null)
                _httpClient.Timeout = _timeout.Value;
        }

        public async Task<T> SendGetAsync<T>(string uri,
                CallOptions options = null)
        {
            using (var resp = await ProcessRequestAsync(HttpMethod.Get, uri, options: options))
            {
                return await ProcessResponseAsync<T>(resp);
            }

            // TODO: PURGE!
            // using (var requ = new HttpRequestMessage(HttpMethod.Get, uri))
            // {
            //     if ((options?.WrapTtl.HasValue).GetValueOrDefault())
            //         requ.Headers.Add(Protocol.WrapTtlHeader, options.WrapTtl);

            //     var task = options?.Cancel == null
            //         ? _httpClient.SendAsync(requ)
            //         : _httpClient.SendAsync(requ, options.Cancel.Value);

            //     using (var resp = await task)
            //     {
            //         return await ProcessResponseAsync<T>(resp);
            //     }
            // }
        }

        public async Task<T> SendPutAsync<T>(string uri, object payload = null,
                CallOptions options = null)
        {
            using (var resp = await ProcessRequestAsync(HttpMethod.Put, uri, payload, options))
            {
                return await ProcessResponseAsync<T>(resp);
            }

            // TODO:
            // var task = cancel == null
            //     ? _httpClient.PutAsync(uri, GetContent(payload))
            //     : _httpClient.PutAsync(uri, GetContent(payload), cancel.Value);

            // using (var resp = await task)
            // {
            //     return await ProcessResponseAsync<T>(resp);
            // }
        }

        public async Task<T> SendPostAsync<T>(string uri, object payload = null,
                CallOptions options = null)
        {
            using (var resp = await ProcessRequestAsync(HttpMethod.Put, uri, payload, options))
            {
                return await ProcessResponseAsync<T>(resp);
            }

            // TODO: PURGE!
            // var task = cancel == null
            //     ? _httpClient.PostAsync(uri, GetContent(payload))
            //     : _httpClient.PostAsync(uri, GetContent(payload), cancel.Value);

            // using (var resp = await task)
            // {
            //     return await ProcessResponseAsync<T>(resp);
            // }
        }

        public async Task<T> SendDeleteAsync<T>(string uri,
                CallOptions options = null)
        {
            using (var resp = await ProcessRequestAsync(HttpMethod.Delete, uri, options: options))
            {
                return await ProcessResponseAsync<T>(resp);
            }

            // TODO: PURGE!
            // var task = cancel == null
            //     ? _httpClient.DeleteAsync(uri)
            //     : _httpClient.DeleteAsync(uri, cancel.Value);

            // using (var resp = await task)
            // {
            //     return await ProcessResponseAsync<T>(resp);
            // }
        }

        public async Task<T> SendListAsync<T>(string uri, object payload = null,
                Func<IResponseContext, T> on404 = null,
                CallOptions options = null)
        {
            using (var resp = await ProcessRequestAsync(ListHttpMethod, uri, payload, options))
            {
                return await ProcessResponseAsync<T>(resp, on404);
            }

            // TODO: PURGE!
            // using (var requ = new HttpRequestMessage(ListHttpMethod, uri) {
            //     Content = GetContent(payload),
            // })
            // {
            //     var task = cancel == null
            //         ? _httpClient.SendAsync(requ)
            //         : _httpClient.SendAsync(requ, cancel.Value);

            //     using (var resp = await task)
            //     {
            //         return await ProcessResponseAsync<T>(resp, allow404);
            //     }
            // }
        }

        private HttpContent GetContent(object payload)
        {
            if (payload == null)
                return null;

            var payloadJson = JsonConvert.SerializeObject(payload);
            var requContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            return requContent;
        }

        private async Task<HttpResponseMessage> ProcessRequestAsync(HttpMethod method,
                string uri,
                object payload = null,
                CallOptions options = null)
        {
            using (var requ = new HttpRequestMessage(method, uri))
            {
                if (payload != null)
                    requ.Content = GetContent(payload);

                if ((options?.WrapTtl.HasValue).GetValueOrDefault())
                    requ.Headers.Add(WrapTtlHeader, options.WrapTtl);

                var task = options?.Cancel == null
                    ? _httpClient.SendAsync(requ)
                    : _httpClient.SendAsync(requ, options.Cancel.Value);

                return await task;
            }
        }

        private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage resp,
                Func<IResponseContext, T> on404 = null)
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
                // See if we can extract any error details from the response body
                ErrorResponse errors = null;
                if (isJson)
                    errors = JsonConvert.DeserializeObject<ErrorResponse>(respBody);

                // Sometimes a "Not Found" response is OK, like with some LIST ops
                if (on404 != null && resp.StatusCode == HttpStatusCode.NotFound)
                {
                    // HTTP 404 is allowed as a successful response assuming
                    // there were not errors returned in the response body
                    if (errors?.Errors.Length == 0)
                    {
                        var ctx = new ResponseContext
                        {
                            IsJsonBody = isJson,
                            Body = respBody,
                        };
                        
                        return on404(ctx);
                    }
                }

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
                    ex.Errors = errors;
                else
                    ex.Data.Add("Content", respBody);

                throw ex;
            }
        }

        #region -- IDisposable Support ==

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
                    _httpClient.Dispose();
                    _httpClient = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProtocolClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion -- IDisposable Support ==


        internal class ResponseContext : IResponseContext
        {
            public bool IsJsonBody
            { get; set; }

            public string Body
            { get; set; }

            // public HttpHeaders Headers
            // { get; set; }
        }
    }
}