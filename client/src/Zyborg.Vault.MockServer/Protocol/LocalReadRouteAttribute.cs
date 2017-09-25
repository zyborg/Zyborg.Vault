using System;

namespace Zyborg.Vault.MockServer.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalReadRouteAttribute : LocalRouteAttribute
    {
        public LocalReadRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}