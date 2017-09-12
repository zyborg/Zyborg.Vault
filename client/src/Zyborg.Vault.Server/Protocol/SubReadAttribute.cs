using System;

namespace Zyborg.Vault.Server.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SubReadAttribute : Attribute
    {
        public SubReadAttribute(string pathPattern)
        {
            PathPattern = pathPattern;
        }

        public string PathPattern
        { get; }
    }
}