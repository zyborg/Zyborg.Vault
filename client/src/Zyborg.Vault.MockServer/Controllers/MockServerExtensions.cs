using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Zyborg.Vault.Model;
using Zyborg.Vault.MockServer.Auth;
using Zyborg.Vault.MockServer.Policy;
using Zyborg.Vault.MockServer.Storage;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.Controllers
{
    public static class MockServerExtensions
    {
        public static void AssertAuthorized(this MockServer server, HttpContext http,
                Dictionary<string, string> parameters = null,
                bool isSudo = false, Func<string, bool> isCreate = null)
        {
            var controller = AuthMountController.From(http) ?? SecretMountController.From(http);
            
            if (controller == null)
                throw new System.Security.SecurityException("permission denied");

            AssertAuthorized(server, controller, parameters, isSudo, isCreate);
        }

        public static void AssertAuthorized(this MockServer server, Controller controller,
                Dictionary<string, string> parameters = null,
                bool isSudo = false, Func<string, bool> isCreate = null)
        {
            var urlHelper = controller.Url;
            var routeData = controller.RouteData;
            var http = controller.HttpContext;
            var auth = AuthContext.From(http);
            var meth = http.Request.Method.ToUpper();

            // Resolve the auth info based on the current token
            // extracted from the current request in HttpContext
            AuthInfo authInfo = null;
            if (!string.IsNullOrEmpty(auth?.Token))
                try { authInfo = server.GetToken(auth.Token).Result; } catch (Exception) {}
            if (authInfo == null)
                throw new System.Security.SecurityException("permission denied");

            // Resolve the request-specific path, we
            // need this first to help resolve cap next
            var path = urlHelper.RouteUrl(routeData.Values);
            path = PathMap<object>.NormalizePath(path);
            // TODO:  remove this hard-codedness
            if (path.StartsWith("v1/"))
                path = path.Substring(3);

            // Resolve the capability
            string cap;
            if (meth == "DELETE")
                cap = Capability.Delete;
            else if (meth == "PUT" || meth == "POST")
                cap = isCreate == null || !isCreate(path) ? Capability.Update : Capability.Create;
            else if (meth == "LIST" || (meth == "GET"
                    && http.Request.Query.ContainsKey(Util.HttpListAttribute.ListQuery)))
                cap = Capability.List;
            else if (meth == "GET")
                cap = Capability.Read;
            else
                throw new InvalidOperationException("cannot decipher capability from method");

            // Finally, assert the authorization based on all these derived elements
            server.AssertAuthorized(cap, path, parameters, isSudo, authInfo.Policies);
        }
    }
}