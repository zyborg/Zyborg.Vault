using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.Backends.Authentication.Models;
using VaultSharp.Backends.Authentication.Models.Token;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="description">
	/// Local state for interacting with a Vault server endpoint.
	/// </para>
	/// </summary>
	public class VaultSession : IDisposable
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string VaultTokenHeaderKey = "X-Vault-Token";

		public const string VaultWrapTimeToLiveHeaderKey = "X-Vault-Wrap-TTL";

		private readonly Uri _vaultAddr;
		private readonly Uri _vaultBase;
		private string _vaultToken;
		private readonly IAuthenticationInfo _authnInfo;
		private readonly HttpDataAccessManager _dataAccessManager;

		/// <summary>
		/// Creates a session with the two minimum, necessary components
		/// provided as parameters.
		/// </summary>
		public VaultSession(string address, string token)
		{
			VaultAddress = address;
			VaultToken = token;

			_vaultAddr = new Uri(address);
			_vaultBase = new Uri(_vaultAddr, "v1/");


			if (!string.IsNullOrWhiteSpace(token))
			{
				_vaultToken = token;
				_authnInfo = new TokenAuthenticationInfo(token);
			}

			_dataAccessManager = new HttpDataAccessManager(_vaultBase);
			VaultClient = VaultClientFactory.CreateVaultClient(new Uri(address), _authnInfo);
		}

		/// <summary>
		/// URL specifying the Vault server endpoint.
		/// </summary>
		public string VaultAddress
		{ get; private set; }

		/// <summary>
		/// The authentication Token to self-identify with to the Vault server.
		/// </summary>
		public string VaultToken
		{ get; private set; }

		public IVaultClient VaultClient
		{ get; private set; }

		public void Dispose()
		{
			_dataAccessManager.Dispose();

			VaultClient = null;
		}

		public async Task<TResponse> ListData<TResponse>(string path)
			where TResponse : class
		{
			return await MakeVaultApiRequest<TResponse>($"{path}?list=true", HttpMethod.Get);
		}

		public async Task<Secret<TResponseData>> ReadSecret<TResponseData>(string path)
			where TResponseData : class
		{
			return await MakeVaultApiRequest<Secret<TResponseData>>(path, HttpMethod.Get);
		}

		public async Task<Secret<TResponseData>> WriteSecret<TResponseData>(string path, object values = null)
			where TResponseData : class
		{
			return await MakeVaultApiRequest<Secret<TResponseData>>(path, HttpMethod.Post, values);
		}

		public async Task<TResponse> MakeVaultApiRequest<TResponse>(string resourcePath,
				HttpMethod httpMethod, object requestData = null, bool rawResponse = false,
				Func<int, string, TResponse> customProcessor = null, string wrapTimeToLive = null)
			where TResponse : class
		{
			var headers = new Dictionary<string, string>();

			if (_vaultToken != null)
				headers.Add(VaultTokenHeaderKey, _vaultToken);

			if (wrapTimeToLive != null)
				headers.Add(VaultWrapTimeToLiveHeaderKey, wrapTimeToLive);

			return await _dataAccessManager.MakeRequestAsync<TResponse>(resourcePath, httpMethod,
					requestData, headers, rawResponse, customProcessor);
		}

		// This class is pulled straight from:
		//    https://github.com/rajanadar/VaultSharp/blob/master/src/VaultSharp/DataAccess/HttpDataAccessManager.cs
		class HttpDataAccessManager : IDisposable
		{
			private readonly HttpClient _httpClient;
			private readonly bool _continueAsyncTasksOnCapturedContext;

			public HttpDataAccessManager(Uri baseAddress, HttpMessageHandler httpMessageHandler = null,
					bool continueAsyncTasksOnCapturedContext = false, TimeSpan? serviceTimeout = null,
					Action<HttpClient> postHttpClientInitializeAction = null)
			{
				_httpClient = httpMessageHandler == null
					? new HttpClient()
				    : new HttpClient(httpMessageHandler);
				_httpClient.BaseAddress = baseAddress;
				_continueAsyncTasksOnCapturedContext = continueAsyncTasksOnCapturedContext;

				if (serviceTimeout != null)
				{
					_httpClient.Timeout = serviceTimeout.Value;
				}

				if (postHttpClientInitializeAction != null)
				{
					postHttpClientInitializeAction(_httpClient);
				}
			}

			public void Dispose()
			{
				_httpClient?.Dispose();
			}

			public async Task<TResponse> MakeRequestAsync<TResponse>(string resourcePath,
					HttpMethod httpMethod, object requestData = null,
					IDictionary<string, string> headers = null, bool rawResponse = false,
					Func<int, string, TResponse> customProcessor = null) where TResponse : class
			{
				try
				{
					var requestUri = new Uri(_httpClient.BaseAddress, resourcePath);

					var requestContent = requestData != null
						? new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8)
						: null;

					HttpRequestMessage httpRequestMessage = null;

					switch (httpMethod.ToString().ToUpperInvariant())
					{
						case "GET":

							httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
							break;

						case "DELETE":

							httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
							break;

						case "POST":

							httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
							{
								Content = requestContent
							};

							break;

						case "PUT":

							httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri)
							{
								Content = requestContent
							};

							break;

						case "HEAD":

							httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, requestUri);
							break;

						default:
							throw new NotSupportedException("The Http Method is not supported: " + httpMethod);
					}

					if (headers != null)
					{
						foreach (var kv in headers)
						{
							httpRequestMessage.Headers.Remove(kv.Key);
							httpRequestMessage.Headers.Add(kv.Key, kv.Value);
						}
					}

					var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage)
							.ConfigureAwait(continueOnCapturedContext: _continueAsyncTasksOnCapturedContext);
					var responseText =
						await
							httpResponseMessage.Content.ReadAsStringAsync()
									.ConfigureAwait(continueOnCapturedContext: _continueAsyncTasksOnCapturedContext);

					if (httpResponseMessage.IsSuccessStatusCode)
					{
						if (!string.IsNullOrWhiteSpace(responseText))
						{
							var response = rawResponse
								? (responseText as TResponse)
								: JsonConvert.DeserializeObject<TResponse>(responseText);
							return response;
						}

						return default(TResponse);
					}

					if (customProcessor != null)
					{
						return customProcessor((int)httpResponseMessage.StatusCode, responseText);
					}

					throw new Exception(string.Format(CultureInfo.InvariantCulture,
						"{0} {1}. {2}",
						(int)httpResponseMessage.StatusCode, httpResponseMessage.StatusCode, responseText));
				}
				catch (WebException ex)
				{
					if (ex.Status == WebExceptionStatus.ProtocolError)
					{
						var response = ex.Response as HttpWebResponse;

						if (response != null)
						{
							string responseText;

							using (StreamReader stream = new StreamReader(response.GetResponseStream()))
							{
								responseText =
									await stream.ReadToEndAsync()
											.ConfigureAwait(continueOnCapturedContext: _continueAsyncTasksOnCapturedContext);
							}

							if (customProcessor != null)
							{
								return customProcessor((int)response.StatusCode, responseText);
							}

							throw new Exception(string.Format(CultureInfo.InvariantCulture,
								"{0} {1}. {2}",
								(int)response.StatusCode, response.StatusCode, responseText), ex);
						}

						throw;
					}

					throw;
				}
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
