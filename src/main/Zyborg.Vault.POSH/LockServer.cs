using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Lock, "Server")]
	[OutputType(typeof(SealStatus))]
	public class LockServer : VaultBaseCmdlet
	{
		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			_client.SealAsync().Wait();
			base.WriteObject(_client.GetSealStatusAsync().Result);
		}
	}
}
