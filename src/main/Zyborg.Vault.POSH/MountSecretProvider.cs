using System.Management.Automation;
using VaultSharp.Backends.Secret.Models;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Mount a logical secrets provider.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>mount</c> and <c>remount</c> commands.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command mounts (or remounts) a logical backend for storing and/or
	/// generating secrets.
	/// </para><para type="description">
	/// When remounting, all the secrets from the old path will be revoked, but the
	/// data associated with the backend(such as configuration), will be preserved.
	/// </para>
	/// </remarks>
	/// <example>
	/// <para>
	/// This shows an example of remounting an existing mount path.
	/// </para><code>
	/// ##
	/// 
	///   Mount-HCVaultSecretProvider -OldMountName secret -NewMountName generic
	/// </code>
	/// </example>
	[Cmdlet(VerbsData.Mount, "SecretProvider", DefaultParameterSetName = DefaultParamSet)]
	public class MountSecretProvider : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string RemountParamSet = "Remount";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The name of the secrets provider to mount (e.g. generic).
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = DefaultParamSet)]
		public string Type
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Mount point for the logical secrets provider.
		/// </para><para type="description">
		/// If not specified, this defaults to the name of the type of the mount provider.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 1, ParameterSetName = DefaultParamSet)]
		public string MountName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Human-friendly description of the purpose for the mount.
		/// This shows up in the list of mounts.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 2, ParameterSetName = DefaultParamSet)]
		public string Description
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Default lease time-to-live for this backend. If not specified,
		/// uses the global default, or the previously set value.  Set to
		/// '0' to explicitly set it to use the global default.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		public string DefaultLeaseTtl
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Max lease time-to-live for this provider mount. If not specified,
		/// uses the global default, or the previously set value.  Set to '0'
		/// to explicitly set it to use the global default.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		public string MaxLeaseTtl
		{ get; set; }

		// TODO: no way to pass this option with VaultSharp Client
		//[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		//public SwitchParameter Local
		//{ get; set; }

		// TODO: no way to pass this option with VaultSharp Client
		//[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		//public SwitchParameter NoCache
		//{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The name that identifies an existing mount point of a secrets provider
		/// to be relocated.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = RemountParamSet)]
		public string OldMountName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The new name to move an existing secrets provider mount.
		/// </para><para type="description">
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = RemountParamSet)]
		public string NewMountName
		{ get; set; }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			if (ParameterSetName == RemountParamSet)
			{
				AsyncWait(_client.RemountSecretBackendAsync(OldMountName, NewMountName));
			}
			else
			{
				if (string.IsNullOrEmpty(MountName))
					MountName = Type;

				MountConfiguration mc = null;
				if (!string.IsNullOrEmpty(DefaultLeaseTtl) || !string.IsNullOrEmpty(MaxLeaseTtl))
				{
					mc = new MountConfiguration
					{
						DefaultLeaseTtl = DefaultLeaseTtl,
						MaximumLeaseTtl = MaxLeaseTtl,
					};
				}

				var sb = new SecretBackend
				{
					BackendType = new SecretBackendType(Type),
					MountPoint = MountName,
					Description = Description,
					MountConfiguration = mc,
				};

				AsyncWait(_client.MountSecretBackendAsync(sb));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
