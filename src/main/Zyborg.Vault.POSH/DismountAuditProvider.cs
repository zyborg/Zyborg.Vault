using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsLifecycle.Disable, "AuditProvider")]
	public class DismountAuditProvider : VaultBaseCmdlet
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
			AsyncWait(_client.DisableAuditBackendAsync(MountName));
		}
	}
}
