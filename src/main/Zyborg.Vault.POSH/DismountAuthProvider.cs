using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">Disable an already-enabled auth provider.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	///	Once the auth provider is disabled its path can no longer be used
	///	to authenticate. All access tokens generated via the disabled auth provider
	///	will be revoked. This command will block until all tokens are revoked.
	///	If the command is exited early the tokens will still be revoked.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsData.Dismount, "AuthProvider")]
	public class DismountAuthProvider : VaultBaseCmdlet
	{
		/// <remarks>
		/// <para type="description">The name that identifies the mount point.</para>
		/// <para type="description">
		/// This should map to the <c>MountName</c> used for
		/// <c>Mount-AuthProvider</c>. If no <c>MountName</c> was provided
		/// when mounting, you should use the provider type (e.g. "file").
		/// </para>
		/// </remarks>
		[Parameter(Mandatory = true, Position = 0)]
		public string MountName
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		
		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			AsyncWait(_client.DisableAuthenticationBackendAsync(MountName));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
