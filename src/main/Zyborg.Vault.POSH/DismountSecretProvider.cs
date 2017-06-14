using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Disable an already-enabled auth provider.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>auth-disable</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Once the auth provider is disabled its path can no longer be used
	/// to authenticate.
	/// </para><para type="description">
	/// All access tokens generated via the disabled auth provider
	/// will be revoked.This command will block until all tokens are revoked.
	/// If the command is exited early the tokens will still be revoked.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsData.Dismount, "SecretProvider")]
	public class DismountSecretProvider : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// One or more paths at which auth providers are mounted.
		/// If an auth provider is mounted at multiple paths, each representing
		/// a different instance (with different configuration), this identifies
		/// the specific instance to disable.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] MountName
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var mn in MountName)
			{
				AsyncWait(_client.UnmountSecretBackendAsync(mn));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
