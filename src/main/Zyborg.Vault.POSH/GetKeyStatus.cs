using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Provides information about the active encryption key.
	/// Specifically, the current key term and the key installation time.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>key-status</c> command.
	/// </para>
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "KeyStatus")]
	[OutputType(typeof(EncryptionKeyStatus))]
	public class GetKeyStatus : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			WriteAsyncResult(_client.GetEncryptionKeyStatusAsync());
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
