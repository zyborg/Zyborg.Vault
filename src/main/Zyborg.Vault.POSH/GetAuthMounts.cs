using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "AuthMounts")]
	[OutputType(typeof(AuthenticationBackend))]
	public class GetAuthMounts : VaultBaseCmdlet
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
			var r = AsyncWaitFor(_client.GetAllEnabledAuthenticationBackendsAsync());
			WriteWrappedEnumerableData(r, KeepSecretWrapper);
		}
	}
}
