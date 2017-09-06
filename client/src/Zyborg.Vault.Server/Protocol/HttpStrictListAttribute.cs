using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Zyborg.Vault.Server.Protocol
{
    public class HttpStrictListAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> Methods = new[] { "LIST" };

        public HttpStrictListAttribute() : base(Methods)
        { }

        /// <param name="template">The route template. May not be null.</param>
        public HttpStrictListAttribute(string template) : base(Methods, template)
        { }
    }
}