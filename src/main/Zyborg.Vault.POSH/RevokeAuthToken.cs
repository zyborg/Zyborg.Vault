using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsSecurity.Revoke, "AuthToken", DefaultParameterSetName = TokenParamSet)]
	public class RevokeAuthToken : VaultBaseCmdlet
	{
		public const string TokenParamSet = "Token";
		public const string AccessorParamSet = "Accessor";
		public const string SelfParamSet = "Self";
		public const string PathPrefixParamSet = "PathPrefix";

		[Parameter(Mandatory = true, Position = 0, ParameterSetName = TokenParamSet)]
		public string Token
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = AccessorParamSet)]
		public string Accessor
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = SelfParamSet)]
		public SwitchParameter Self
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PathPrefixParamSet)]
		public string PathPrefix
		{ get; set; }

		[Parameter(Mandatory = false, ParameterSetName = TokenParamSet)]
		public SwitchParameter OrphanChildren
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			if (ParameterSetName == PathPrefixParamSet)
			{
				AsyncWait(_client.RevokeAllSecretsOrTokensUnderPrefixAsync(PathPrefix));
			}
			else if (ParameterSetName == SelfParamSet)
			{
				AsyncWait(_client.RevokeCallingTokenAsync());
			}
			else if (ParameterSetName == AccessorParamSet)
			{
				AsyncWait(_client.RevokeTokenByAccessorAsync(Accessor));
			}
			else
			{
				AsyncWait(_client.RevokeTokenAsync(Token, !OrphanChildren));
			}
		}
	}
}
