using System.Collections;
using System.Linq;
using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsLifecycle.Enable, "AuthProvider")]
	public class MountAuthProvider : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0)]
		public string Type
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1)]
		public string MountName
		{ get; set; }

		[Parameter(Mandatory = false, Position = 2)]
		public string Description
		{ get; set; }

		// TODO: no way to pass this option with VaultSharp Client
		//[Parameter(Mandatory = false)]
		//public SwitchParameter Local
		//{ get; set; }

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
	}
}
