using System.Collections;
using System.Management.Automation;
using Zyborg.Vault.POSH.Model;

namespace Zyborg.Vault.POSH
{
	/// <summary >
	/// <para type="synopsis">Enable an audit provider.</para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command enables an audit backend of type <c>Type</c>. Additional
	/// options for configuring the audit provider can be specified after the
	/// type in the same format as the <c>Write-Data</c> command in key/value pairs.
	/// </para><para type="description">
	/// For information on available configuration options, please see the
	/// documentation.
	/// </para>
	/// </remarks>
	/// <example>
	/// <para>
	/// To configure the file audit provider to write audit logs at the
	/// path c:\temp\log\audit.log:
	/// </para><code>
	/// ##
	/// 
	///	  PS> Mount-HCVaultAuditProvider file -Config @{ file_path = "c:\temp\log\audit.log" }
	/// </code>
	/// </example>
	[Cmdlet(VerbsLifecycle.Enable, "AuditProvider")]
	public class MountAuditProvider : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">The type name of the audit provider (e.g. file).</para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0)]
		public string Type
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specify a unique path for this audit provider. This
		/// is purely for referencing this audit provider. By
		/// default this will be the backend type.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 1)]
		public string MountName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// One or more configuration key-value settings.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 2)]
		public Hashtable Config
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// A human-friendly description for the provider. This
		/// shows up only when querying the enabled providers.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 3)]
		public string Description
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// TODO:  This flag is NOT currently supported.
		/// </para>
		/// </summary>

		// TODO: no way to pass this option with VaultSharp Client
		[Parameter(Mandatory = false)]
		public SwitchParameter Local
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			var ab = new GenericAuditBackend(Type)
			{
				MountPoint = MountName,
				Options = Config,
				Description = Description,
			};

			AsyncWait(_client.EnableAuditBackendAsync(ab));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
