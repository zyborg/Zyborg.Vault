using System;

namespace Zyborg.Vault.Server.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalListRouteAttribute : LocalRouteAttribute
    {
        public LocalListRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}