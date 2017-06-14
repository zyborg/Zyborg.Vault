using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Gets the initialization status of Vault.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This is an unauthenticated call and does not use a vault auth token.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsDiagnostic.Test, "Instance")]
	public class TestInstance : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			base.WriteAsyncResult(_client.GetInitializationStatusAsync());
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
