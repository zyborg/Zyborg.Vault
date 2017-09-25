using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zyborg.Vault.MockServer.Auth;

namespace Zyborg.Vault.MockServer
{
    /// <summary>
    /// Middleware to perform some validation and adjustments to the HTTP request
    /// based on our behavior requirements.
    /// </summary>
    /// <remarks>
    /// Mock Server uses MVC to do model binding for all input body content which
    /// relies on a request Content-type to properly parse the content.  Unfortunately,
    /// the Vault CLI often (usually?) sends its write-oriented operations with a JSON
    /// payload but does not provide a Content-type, which breaks the model binding so
    /// this middleware adjusts any request that has uses a <c>PUT</c> or <c>POST</c>
    /// HTTP method but no explicit Content-type header, in which case it defaults to
    /// JSON.
    /// </remarks>
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext ctx)
        {
            // Setup an AuthContext for the duration of the request
            AuthContext.BuildFrom(ctx, true);

            var cLen = ctx.Request.ContentLength;
            var cType = ctx.Request.ContentType;
            var method = ctx.Request.Method;

            if (HttpMethods.IsPut(method) || HttpMethods.IsPost(method))
                if (string.IsNullOrEmpty(cType))
                    if (cLen.HasValue)
                        ctx.Request.ContentType = "application/json";

            await _next(ctx);
        }
    }
}