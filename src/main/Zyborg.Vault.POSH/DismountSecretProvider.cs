using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsData.Dismount, "SecretProvider")]
	public class DismountSecretProvider : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] MountName
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var mn in MountName)
			{
				AsyncWait(_client.UnmountSecretBackendAsync(mn));
			}
		}
	}
}
