using System.Collections;
using System.Management.Automation;
using Zyborg.Vault.POSH.Model;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsLifecycle.Enable, "AuditProvider")]
	public class EnableAuditProvider : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0)]
		public string Type
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1)]
		public string MountName
		{ get; set; }

		[Parameter(Mandatory = false, Position = 2)]
		public Hashtable Config
		{ get; set; }

		[Parameter(Mandatory = false, Position = 3)]
		public string Description
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter Local
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			var ab = new GenericAuditBackend(Type)
			{
				MountPoint = MountName,
				Options = Config,
				Description = Description,
			};

			_client.EnableAuditBackendAsync(ab).Wait();
		}
	}
}
