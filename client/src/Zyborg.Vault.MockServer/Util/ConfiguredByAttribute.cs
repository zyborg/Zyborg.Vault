using System;

namespace Zyborg.Vault.MockServer.Util
{
    /// <summary>
    /// This attribute allows you to specify what type is used to configure
    /// the behavior of another type instance, such as a service provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ConfiguredByAttribute : Attribute
    {
        public ConfiguredByAttribute(Type type)
        {
            this.Type = type;
        }

        public Type Type
        { get; }
    }
}