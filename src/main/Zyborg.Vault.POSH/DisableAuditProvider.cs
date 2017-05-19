using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsLifecycle.Disable, "AuditProvider")]
	public class DisableAuditProvider : VaultBaseCmdlet
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
			_client.DisableAuditBackendAsync(MountName).Wait();
		}
	}
}
