using System;

namespace Zyborg.Vault.MockServer.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalListRouteAttribute : LocalRouteAttribute
    {
        public LocalListRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}