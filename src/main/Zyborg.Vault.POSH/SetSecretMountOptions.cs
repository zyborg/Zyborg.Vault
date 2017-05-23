using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Backends.Secret.Models;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Set, "SecretMountOptions")]
	public class SetSecretMountOptions : VaultBaseCmdlet
	{
		[Parameter(Mandatory = false, Position = 0)]
		public string MountName
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string DefaultLeaseTtl
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string MaxLeaseTtl
		{ get; set; }

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
	}
}
