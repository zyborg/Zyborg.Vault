using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Zyborg.Vault.Server.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class HttpListAttribute : Attribute, IActionConstraint, IRouteTemplateProvider
    {
        public const string ListMethod = "LIST";
        public const string GetMethod = "GET";
        public const string ListQuery = "list";

        private int _Order;

        public HttpListAttribute()
        { }

        /// <param name="template">The route template. May not be null.</param>
        public HttpListAttribute(string template)
        {
            Template = template;
        }

        public string Template
        { get; }

        public int Order
        {
            get => _Order;
            set => _Order = value;
        }

        public string Name
        { get; set; }

        int? IRouteTemplateProvider.Order => _Order;

        public bool Accept(ActionConstraintContext context)
        {
            var method = context.RouteContext.HttpContext.Request.Method;
            var query = context.RouteContext.HttpContext.Request.Query?.ContainsKey(ListQuery);

            return ListMethod == method || (GetMethod == method && query.GetValueOrDefault());
        }
    }
}