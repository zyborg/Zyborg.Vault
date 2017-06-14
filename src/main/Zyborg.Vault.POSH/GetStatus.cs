using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Outputs the state of the Vault, sealed or unsealed and if HA is enabled.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>status</c> command.
	/// </para>
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "Status")]
	[OutputType(typeof(SealStatus))]
	public class GetStatus : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			WriteAsyncResult(_client.GetSealStatusAsync());
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
