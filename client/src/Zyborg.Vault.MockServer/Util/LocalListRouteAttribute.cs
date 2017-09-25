using System;

namespace Zyborg.Vault.MockServer.Util
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalListRouteAttribute : LocalRouteAttribute
    {
        public LocalListRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}