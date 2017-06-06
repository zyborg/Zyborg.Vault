using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsSecurity.Revoke, "Secret", DefaultParameterSetName = LeaseIdParamSet)]
	public class RevokeSecret : VaultBaseCmdlet
	{
		public const string LeaseIdParamSet = "Token";
		public const string PathPrefixParamSet = "PathPrefix";

		[Parameter(Mandatory = true, Position = 0, ParameterSetName = LeaseIdParamSet)]
		public string LeaseId
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PathPrefixParamSet)]
		public string PathPrefix
		{ get; set; }

		[Parameter(Mandatory = false, ParameterSetName = PathPrefixParamSet)]
		public SwitchParameter Force
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			if (ParameterSetName == PathPrefixParamSet)
			{
				if (Force)
				{
					AsyncWait(_client.ForceRevokeAllSecretsOrTokensUnderPrefixAsync(PathPrefix));
				}
				else
				{
					AsyncWait(_client.RevokeAllSecretsOrTokensUnderPrefixAsync(PathPrefix));
				}
			}
			else
			{
				AsyncWait(_client.RevokeSecretAsync(LeaseId));
			}
		}
	}
}
