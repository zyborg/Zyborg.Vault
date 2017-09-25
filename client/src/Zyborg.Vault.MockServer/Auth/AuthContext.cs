using Microsoft.AspNetCore.Http;
using Zyborg.Vault.Model;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.MockServer.Auth
{
    public class AuthContext
    {
        private HttpContext _http;

        /// <summary>
        /// Builds an Authentication/Authorization context based on the
        /// current HttpContext and optionally attaches itself to it.
        /// </summary>
        private AuthContext(HttpContext http)
        {
            _http = http;

            if (_http.Request.Headers.TryGetValue(ProtocolClient.TokenHeader, out var vals))
                Token = vals;
            if (_http.Request.Headers.TryGetValue(ProtocolClient.WrapTtlHeader, out vals))
                WrapTtl = (string)vals;

            RequestProtocol = http.Request.Protocol;
            RequestMethod = http.Request.Method;
            RequestPath = http.Request.Path;
        }

        public string Token
        { get; }

        public Duration? WrapTtl
        { get; }

        public string RequestProtocol
        { get; }

        public string RequestMethod
        { get; }

        public string RequestPath
        { get; }

        public static AuthContext BuildFrom(HttpContext http, bool attach = false)
        {
            var auth = new AuthContext(http);
            if (attach)
                http.Items[typeof(AuthContext)] = auth;

            return auth;
        }

        public static AuthContext From(HttpContext http)
        {
            return http.Items[typeof(AuthContext)] as AuthContext;
        }
    }
}