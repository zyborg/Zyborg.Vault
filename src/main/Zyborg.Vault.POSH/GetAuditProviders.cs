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
			var r = AsyncWaitFor(_client.GetAllEnabledAuditBackendsAsync());
			WriteWrappedEnumerableData(r, KeepSecretWrapper);
		}
	}
}
