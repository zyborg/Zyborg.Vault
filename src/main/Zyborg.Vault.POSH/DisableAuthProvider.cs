using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsLifecycle.Disable, "AuthProvider")]
	public class DisableAuthProvider : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0)]
		public string MountName
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			AsyncWait(_client.DisableAuthenticationBackendAsync(MountName));
		}
	}
}
