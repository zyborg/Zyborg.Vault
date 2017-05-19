using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "Profile",
			DefaultParameterSetName = DefaultParamSet)]
	public class GetProfile : PSCmdlet
	{
		public const string DefaultParamSet = VaultBaseCmdlet.DefaultParamSet;
		public const string GetDetailsParamSet = "GetDetails";

		private string _profileDir;

		[Parameter(Mandatory = true, Position = 0,
				ParameterSetName = GetDetailsParamSet)]
		public string[] VaultProfile
		{ get; set; }

		protected override void BeginProcessing()
		{
			_profileDir = base.InvokeCommand.ExpandString(Global.VaultProfilesDir);
			WriteVerbose($"Resolved user profiles root directory [{_profileDir}]");
		}

		protected override void ProcessRecord()
		{
			var dir = new DirectoryInfo(_profileDir);
			if (!dir.Exists)
			{
				WriteVerbose("Missing user profiles root directory");
				return;
			}

			if (VaultProfile == null)
			{
				var wildcard = string.Format(Global.VaultProfileFileFormat, "*");
				var regex = string.Format(Global.VaultProfileFileFormat.Replace(".", "\\."), "(.+)");

				foreach (var f in dir.GetFiles(wildcard))
				{
					var m = Regex.Match(f.Name, regex);
					if (m.Success)
					{
						WriteObject(m.Groups[1].Value);
					}
				}
			}
			else
			{
				var gpr = new GetProfileResult();
				foreach (var name in VaultProfile)
				{
					var profileFile = Path.Combine(dir.FullName,
							string.Format(Global.VaultProfileFileFormat, VaultProfile));
					WriteVerbose($"Resolved user profile file [{profileFile}]");

					if (File.Exists(profileFile))
					{
						var vp = JsonConvert.DeserializeObject<VaultProfile>(
								File.ReadAllText(profileFile));
						gpr.VaultAddress = vp.VaultAddress;
						gpr.SetVaultToken(vp.VaultToken);
						WriteObject(gpr);
					}
				}
			}
		}

		public class GetProfileResult
		{
			public string VaultAddress
			{ get; set; }

			public SecureString VaultToken
			{ get; set; }

			public void SetVaultToken(string t)
			{
				if (string.IsNullOrEmpty(t))
					VaultToken = null;
				else
				{
					var ss = new SecureString();
					foreach (var c in t)
						ss.AppendChar(c);
					VaultToken = ss;
				}
			}
		}
	}
}
