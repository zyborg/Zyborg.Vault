using System.Management.Automation;
using System.Security;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "Profile",
			DefaultParameterSetName = DefaultParamSet)]
	public class GetProfile : PSCmdlet
	{
		public const string DefaultParamSet = VaultBaseCmdlet.DefaultParamSet;
		public const string GetDetailsParamSet = "GetDetails";

		[Parameter(Mandatory = true, Position = 0,
				ParameterSetName = GetDetailsParamSet)]
		public string[] VaultProfile
		{ get; set; }

		protected override void ProcessRecord()
		{
			if (VaultProfile == null)
			{
				foreach (var name in Global.GetVaultProfileNames(this))
				{
					WriteObject(name);
				}
			}
			else
			{
				foreach (var name in VaultProfile)
				{
					var vp = Global.GetVaultProfile(this, name);
					if (vp != null)
					{
						WriteObject(new GetProfileResult
						{
							VaultAddress = vp.VaultAddress,
						}.SetVaultToken(vp.VaultToken));
					}
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

		public GetProfileResult SetVaultToken(string t)
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

			return this;
		}
	}
}
