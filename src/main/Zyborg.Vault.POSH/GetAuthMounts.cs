using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// List the available auth provider mounts and auth methods.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'auth'</c> command with the <c>'-methods'</c> parameter.
	/// </para>
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "AuthMounts")]
	[OutputType(typeof(AuthenticationBackend))]
	public class GetAuthMounts : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// When specified, the returned result maintains the meta data wrapper
		/// for the secret result.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var r = AsyncWaitFor(_client.GetAllEnabledAuthenticationBackendsAsync());
			WriteWrappedEnumerableData(r, KeepSecretWrapper);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
