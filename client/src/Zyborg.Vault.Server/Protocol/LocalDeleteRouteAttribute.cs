using System;

namespace Zyborg.Vault.Server.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalDeleteRouteAttribute : LocalRouteAttribute
    {
        public LocalDeleteRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}