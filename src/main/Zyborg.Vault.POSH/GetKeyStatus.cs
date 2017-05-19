using Newtonsoft.Json;
using System.Management.Automation;
using System.Net.Http;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "KeyStatus")]
	[OutputType(typeof(EncryptionKeyStatus))]
	public class GetKeyStatus : VaultBaseCmdlet
	{
		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var r = _client.GetEncryptionKeyStatusAsync().Result;
			base.WriteObject(r);
		}
	}
}
