using System.Collections;
using System.Linq;
using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsLifecycle.Enable, "AuthProvider")]
	public class EnableAuthProvider : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0)]
		public string Type
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1)]
		public string Path
		{ get; set; }

		[Parameter(Mandatory = false, Position = 2)]
		public string Description
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter Local
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

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			if (string.IsNullOrEmpty(Path))
				Path = Type;

			var ab = new AuthenticationBackend()
			{
				BackendType = new AuthenticationBackendType(Type),
				AuthenticationPath = Path,
				Description = Description,
			};

			_client.EnableAuthenticationBackendAsync(ab).Wait();

			if (Config != null)
			{
				var values = Config.Keys.Cast<string>().ToDictionary(
						x => x, x => Config[x]);
				_client.WriteRawSecretAsync($"auth/{Path}/config", values).Wait();
			}
		}
	}
}
