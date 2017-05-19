using System.Management.Automation;
using VaultSharp.Backends.Audit.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "AuditProviders")]
	[OutputType(typeof(AuditBackend))]
	public class GetAuditProviders : VaultBaseCmdlet
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
			var r = _client.GetAllEnabledAuditBackendsAsync().Result;

			if (KeepSecretWrapper.IsPresent)
				base.WriteObject(r);
			else
				foreach (var ab in r.Data)
					base.WriteObject(ab);
		}
	}
}
