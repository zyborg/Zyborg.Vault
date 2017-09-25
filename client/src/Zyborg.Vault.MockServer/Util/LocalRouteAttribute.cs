using System;

namespace Zyborg.Vault.MockServer.Util
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class LocalRouteAttribute : Attribute
    {
        public LocalRouteAttribute(string pathPattern)
        {
            PathPattern = pathPattern;
        }

        public string PathPattern
        { get; }

        public string Name
        { get; set; }
    }
}