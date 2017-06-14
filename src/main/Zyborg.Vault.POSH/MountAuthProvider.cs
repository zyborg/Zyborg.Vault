using System.Collections;
using System.Linq;
using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">Enable a new auth provider.</para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command enables a new auth provider. An auth provider is responsible
	/// for authenticating a user and assigning them policies with which they can
	/// access Vault.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsLifecycle.Enable, "AuthProvider")]
	public class MountAuthProvider : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// The type name of the auth provider (e.g. userpass).
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0)]
		public string Type
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Mount point for the auth provider. This defaults
		/// to the type of the mount. This will make the auth
		/// provider available at <c>/auth/[path]</c>.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 1)]
		public string MountName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Human-friendly description of the purpose of the
		/// auth provider. This shows up in the <c>Get-AuthMounts</c>.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 2)]
		public string Description
		{ get; set; }

		/// <summary>
		/// Optionally, specifies configuration values to apply to the enabled auth provider.
		/// </summary>
		/// <remarks>
		/// This option <i>assumes</i> that the authentication provider type supports a
		/// configuration child element name <c>config</c>.  While many auth providers to
		/// conform to this pattern, some do not and require specialized configuration
		/// steps.  <b>Please consult the documentation for each specific authentication
		/// provider type about its specific configuration requirements.</b>
		/// </remarks>
		[Parameter(Mandatory = false)]
		public Hashtable Config
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
			if (string.IsNullOrEmpty(MountName))
				MountName = Type;

			var ab = new AuthenticationBackend()
			{
				BackendType = new AuthenticationBackendType(Type),
				AuthenticationPath = MountName,
				Description = Description,
			};

			AsyncWait(_client.EnableAuthenticationBackendAsync(ab));

			if (Config != null)
			{
				var values = Config.Keys.Cast<string>().ToDictionary(
						x => x, x => Config[x]);
				AsyncWait(_client.WriteRawSecretAsync($"auth/{MountName}/config", values));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
