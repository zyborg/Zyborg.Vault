using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsData.Dismount, "SecretProvider")]
	public class DismountSecretProvider : VaultBaseCmdlet
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
			AsyncWait(_client.UnmountSecretBackendAsync(MountName));
		}
	}
}
