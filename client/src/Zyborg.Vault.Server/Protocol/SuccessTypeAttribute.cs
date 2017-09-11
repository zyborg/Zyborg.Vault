using System;

namespace Zyborg.Vault.Server.Protocol
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