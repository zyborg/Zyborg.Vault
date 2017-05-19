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
			var profileDir = base.InvokeCommand.ExpandString(Global.VaultProfilesDir);
			WriteVerbose($"Resolved user profiles root directory [{profileDir}]");

			var profileFile = Path.Combine(profileDir,
					string.Format(Global.VaultProfileFileFormat, VaultProfile));
			WriteVerbose($"Resolved user profile file [{profileFile}]");

			if (Remove.IsPresent)
			{
				if (File.Exists(profileFile))
				{
					WriteVerbose("Removing profile file");
					File.Delete(profileFile);
				}
			}
			else
			{
				if (!Directory.Exists(profileDir))
				{
					// TODO: need to provide a default Directory ACL to
					// protect the profiles directory

					WriteVerbose("Creating user profiles root directory");
					Directory.CreateDirectory(profileDir);
				}

				if (File.Exists(profileFile) && !Force.IsPresent)
					throw new Exception("Existing profile found, use -Force to overwrite");

				var vp = new VaultProfile
				{
					VaultAddress = VaultAddress,
					VaultToken = VaultToken,
				};

				WriteVerbose("Saving VaultProfile to file");
				File.WriteAllText(profileFile, JsonConvert.SerializeObject(vp));
			}
		}
	}
}
