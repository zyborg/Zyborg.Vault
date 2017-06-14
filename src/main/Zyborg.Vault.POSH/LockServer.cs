using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Seal the vault.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'seal'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Sealing a vault tells the Vault server to stop responding to any
	/// access operations until it is unsealed again.  A sealed vault throws away
	/// its master key to unlock the data, so it is physically blocked from
	/// responding to operations again until the vault is unsealed with
	/// the "unseal" command or via the API.
	/// </para><para type="description">
	/// This command is idempotent, if the vault is already sealed it does nothing.
	/// </para><para type="description">
	/// If an unseal has started, sealing the vault will reset the unsealing
	/// process. You'll have to re-enter every portion of the master key again.
	/// </para>
	/// <para type="link">Unlock-HCVaultServer</para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Lock, "Server")]
	[OutputType(typeof(SealStatus))]
	public class LockServer : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			AsyncWait(_client.SealAsync());
			WriteAsyncResult(_client.GetSealStatusAsync());
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
