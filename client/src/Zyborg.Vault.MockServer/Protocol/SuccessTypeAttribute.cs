using System;

namespace Zyborg.Vault.MockServer.Protocol
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SuccessTypeAttribute : Attribute
    {
        public SuccessTypeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type
        { get; }
    }
}