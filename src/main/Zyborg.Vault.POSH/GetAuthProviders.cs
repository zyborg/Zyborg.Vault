using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "AuthProviders")]
	[OutputType(typeof(AuthenticationBackend))]
	public class GetAuthProviders : VaultBaseCmdlet
	{
		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var r = _client.GetAllEnabledAuthenticationBackendsAsync().Result;

			if (KeepSecretWrapper.IsPresent)
				base.WriteObject(r);
			else
				foreach (var ab in r.Data)
					base.WriteObject(ab);
		}
	}
}
