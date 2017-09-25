using System;

namespace Zyborg.Vault.MockServer.Util
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LocalWriteRouteAttribute : LocalRouteAttribute
    {
        public LocalWriteRouteAttribute(string pathPattern) : base(pathPattern)
        { }
    }
}