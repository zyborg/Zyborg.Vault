using System;

namespace Zyborg.Vault.Server.Protocol
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SubWriteAttribute : Attribute
    {
        public SubWriteAttribute(string pathPattern)
        {
            PathPattern = pathPattern;
        }

        public string PathPattern
        { get; }
    }
}