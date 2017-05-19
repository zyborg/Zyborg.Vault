using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "Status")]
	[OutputType(typeof(SealStatus))]
	public class GetStatus : VaultBaseCmdlet
	{
		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			base.WriteObject(_client.GetSealStatusAsync().Result);
		}
	}
}
