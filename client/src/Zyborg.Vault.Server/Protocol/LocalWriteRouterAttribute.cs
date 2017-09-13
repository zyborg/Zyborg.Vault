using System;

namespace Zyborg.Vault.Server.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalWriteRouteAttribute : LocalRouteAttribute
    {
        public LocalWriteRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}