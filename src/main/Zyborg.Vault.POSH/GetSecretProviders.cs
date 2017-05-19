using System.Management.Automation;
using VaultSharp.Backends.Secret.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "SecretProviders")]
	[OutputType(typeof(SecretBackend))]
	public class GetSecretProviders : VaultBaseCmdlet
	{
		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		protected override void BeginProcessing()
		{
			base.ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var r = _client.GetAllMountedSecretBackendsAsync().Result;

			if (KeepSecretWrapper.IsPresent)
				base.WriteObject(r);
			else
				foreach (var sb in r.Data)
					base.WriteObject(sb);
		}
	}
}
