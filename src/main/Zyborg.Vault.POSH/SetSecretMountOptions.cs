using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Tune configuration options for a mounted secret provider.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'mount-tune'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// </para><para type="description">
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// ##
	///   Set-HCVaultSecretMountOptions -DefaultLeaseTtl 24h secret
	/// </code>
	/// </example>
	[Cmdlet(VerbsCommon.Set, "SecretMountOptions")]
	public class SetSecretMountOptions : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// The name of the mount point to adjust configuration options for.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 0)]
		public string MountName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Default lease time-to-live for this backend.
		/// </para><para type="description">
		/// If not specified, uses the system default, or the previously set value.
		/// Set to 'system' to explicitly set it to use the system default.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string DefaultLeaseTtl
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Max lease time-to-live for this backend.
		/// </para><para type="description">
		/// If not specified, uses the system default, or the previously set value.
		/// Set to 'system' to explicitly set it to use the system default.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string MaxLeaseTtl
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			MountConfiguration mc = null;
			if (!string.IsNullOrEmpty(DefaultLeaseTtl) || !string.IsNullOrEmpty(MaxLeaseTtl))
			{
				mc = new MountConfiguration
				{
					DefaultLeaseTtl = DefaultLeaseTtl,
					MaximumLeaseTtl = MaxLeaseTtl,
				};
			}

			AsyncWait(_client.TuneSecretBackendConfigurationAsync(MountName, mc));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
