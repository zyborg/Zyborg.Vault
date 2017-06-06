using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsData.Update, "AuthToken", DefaultParameterSetName = TokenParamSet)]
	public class UpdateAuthToken : VaultBaseCmdlet
	{
		public const string TokenParamSet = "Token";
		public const string SelfParamSet = "Self";
		
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = TokenParamSet)]
		public string Token
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = SelfParamSet)]
		public SwitchParameter Self
		{ get; set; }

		[Parameter(Mandatory = false)]
		public int? Increment
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			if (ParameterSetName == SelfParamSet)
			{
				// TODO:  Forces us to pass in using integer seconds
				AsyncWait(_client.RenewCallingTokenAsync(Increment));
			}
			else
			{
				// TODO:  Forces us to pass in using integer seconds
				AsyncWait(_client.RenewTokenAsync(Token, Increment));
			}
		}
	}
}
