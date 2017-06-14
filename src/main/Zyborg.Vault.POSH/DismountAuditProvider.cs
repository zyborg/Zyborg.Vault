using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary >
	/// <para type="synopsis">Disable an audit provider.</para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Once the audit backend is disabled no more audit logs will be sent to
	/// it. The data associated with the audit backend isn't affected.
	/// </para><para type="description">
	/// The <see cref="MountName"/> parameter should map to the
	/// <see cref="MountAuditProvider.MountName"/>used in
	/// <see cref="MountAuditProvider"/>. If no <c>MountName</c> was provided
	/// when mounting, you should use the provider type (e.g. "file").
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsData.Dismount, "AuditProvider")]
	public class DismountAuditProvider : VaultBaseCmdlet
	{
		/// <remarks>
		/// <para type="description">The name that identifies the mount point.</para>
		/// <para type="description">
		/// This should map to the <c>MountName</c> used for
		/// <c>Mount-AuditProvider</c>. If no <c>MountName</c> was provided
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
			AsyncWait(_client.DisableAuditBackendAsync(MountName));
		}
		
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
