using System;

namespace Zyborg.Vault.MockServer.Util
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