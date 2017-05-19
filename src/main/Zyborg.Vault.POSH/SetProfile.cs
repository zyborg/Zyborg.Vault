using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Set, "Profile",
			DefaultParameterSetName = DefaultParamSet)]
	public class SetProfile : PSCmdlet
	{
		public const string DefaultParamSet = VaultBaseCmdlet.DefaultParamSet;
		public const string RemoveParamSet = "Remove";

		[Parameter(Mandatory = true, Position = 0)]
		public string VaultProfile
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1,
				ParameterSetName = DefaultParamSet)]
		public string VaultAddress
		{ get; set; }

		[Parameter(Mandatory = false, Position = 2,
				ParameterSetName = DefaultParamSet)]
		public string VaultToken
		{ get; set; }

		[Parameter(Mandatory = false,
				ParameterSetName = DefaultParamSet)]
		public SwitchParameter Force
		{ get; set; }

		[Parameter(Mandatory = true,
				ParameterSetName = RemoveParamSet)]
		public SwitchParameter Remove
		{ get; set; }


		protected override void EndProcessing()
		{
			SetVaultProfile(this, VaultProfile, Remove.IsPresent,
					Force.IsPresent, VaultAddress, VaultToken);
		}

		public static void SetVaultProfile(PSCmdlet ctx, string name, bool Remove = false,
				bool force = false, string vaultAddress = null, string vaultToken = null)
		{
			var profileDir = ctx.InvokeCommand.ExpandString(Global.VaultProfilesDir);
			ctx.WriteVerbose($"Resolved user profiles root directory [{profileDir}]");

			var profileFile = Path.Combine(profileDir,
					string.Format(Global.VaultProfileFileFormat, name));
			ctx.WriteVerbose($"Resolved user profile file [{profileFile}]");

			if (Remove)
			{
				if (File.Exists(profileFile))
				{
					ctx.WriteVerbose("Removing profile file");
					File.Delete(profileFile);
				}
			}
			else
			{
				if (!Directory.Exists(profileDir))
				{
					// TODO: need to provide a default Directory ACL to
					// protect the profiles directory

					ctx.WriteVerbose("Creating user profiles root directory");
					Directory.CreateDirectory(profileDir);
				}

				if (File.Exists(profileFile) && !force)
					throw new Exception("Existing profile found, use -Force to overwrite");

				var vp = new VaultProfile
				{
					VaultAddress = vaultAddress,
					VaultToken = vaultToken,
				};

				ctx.WriteVerbose("Saving VaultProfile to file");
				File.WriteAllText(profileFile, JsonConvert.SerializeObject(vp));
			}
		}
	}
}
