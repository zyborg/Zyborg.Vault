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
	[Cmdlet(VerbsData.Mount, "SecretProvider", DefaultParameterSetName = DefaultParamSet)]
	public class MountSecretProvider : VaultBaseCmdlet
	{
		public const string RemountParamSet = "Remount";

		[Parameter(Mandatory = true, Position = 0, ParameterSetName = DefaultParamSet)]
		public string Type
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1, ParameterSetName = DefaultParamSet)]
		public string MountName
		{ get; set; }

		[Parameter(Mandatory = false, Position = 2, ParameterSetName = DefaultParamSet)]
		public string Description
		{ get; set; }

		[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		public string DefaultLeaseTtl
		{ get; set; }

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

		[Parameter(Mandatory = true, ParameterSetName = RemountParamSet)]
		public string OldMountName
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = RemountParamSet)]
		public string NewMountName
		{ get; set; }


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
	}
}
