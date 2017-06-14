using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Unseal the vault by entering a portion of the master key.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'unseal'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Once all portions are entered, the vault will be unsealed.
	/// </para><para type="description">
	/// Every Vault server initially starts as sealed. It cannot perform any
	/// operation except unsealing until it is unsealed. Secrets cannot be accessed
	/// in any way until the vault is unsealed.  This command allows you to enter
	/// a portion of the master key to unseal the vault.
	/// </para><para type="description">
	/// In order to abort the unsealing process and discard any prior keys
	/// that have already been entered in the process, use the
	/// <c>'Lock-HCVaultServer'</c> command.
    /// </para>
	/// <para type="link">Lock-HCVaultServer</para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Unlock, "Server")]
	[OutputType(typeof(SealStatus))]
	public class UnlockServer : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// One or more unseal keys to apply to the unseal process.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Key
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var k in Key)
				WriteAsyncResult(_client.UnsealAsync(k));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
