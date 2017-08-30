using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.GenericSecret
{
    public class GenericSecretOptions : CallOptions
    {
         /// <summary>
        /// Overrides the default mount path of the secret backend being
        /// targeted.
        /// </summary>
        public string MountName
        { get; set; }
       
    }
}