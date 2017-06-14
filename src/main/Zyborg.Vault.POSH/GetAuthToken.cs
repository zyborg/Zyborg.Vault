using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Backends.System.Models;
using Zyborg.Vault.POSH.Model;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "AuthToken", DefaultParameterSetName = TokenParamSet)]
	public class GetAuthToken : VaultBaseCmdlet
	{
		public const string TokenParamSet = "Token";
		public const string AccessorParamSet = "Accessor";
		public const string SelfParamSet = "Self";
		public const string ListParamSet = "List";

		[Parameter(Mandatory = true, Position = 0, ParameterSetName = TokenParamSet)]
		public string Token
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = AccessorParamSet)]
		public string Accessor
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = SelfParamSet)]
		public SwitchParameter Self
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = ListParamSet)]
		public SwitchParameter ListAccessors
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			// NOTE:  The native implementations of these actions in the VaultSharp library
			//        return inconsistent response types with certain common proeperties not
			//        handled uniformly, so instead we implement them using lower-level
			//        primitives and deserialize them with our own model type


			if (ParameterSetName == ListParamSet)
			{
				var r = AsyncWaitFor(_session.ListData<Secret<TokenAccessorList>>(
						"auth/token/accessors"));
				WriteWrappedData(r, KeepSecretWrapper);
			}
			else if (ParameterSetName == AccessorParamSet)
			{
				//var a = AsyncWaitFor(_client.GetTokenInfoByAccessorAsync(Accessor));
				//base.WriteWrappedData(a, KeepSecretWrapper);

				//var r = AsyncWaitFor(_client.WriteSecretAsync($"auth/token/lookup-accessor/{Accessor}", null));
				var r = AsyncWaitFor(_session.WriteSecret<TokenInfo>($"auth/token/lookup-accessor/{Accessor}"));
				WriteWrappedData(r, KeepSecretWrapper);
			}
			else if (ParameterSetName == SelfParamSet)
			{
				//var a = AsyncWaitFor(_client.GetCallingTokenInfoAsync());
				//base.WriteWrappedData(a, KeepSecretWrapper);

				//var r = AsyncWaitFor(_client.ReadSecretAsync($"auth/token/lookup-self"));
				var r = AsyncWaitFor(_session.ReadSecret<TokenInfo>($"auth/token/lookup-self"));
				WriteWrappedData(r, KeepSecretWrapper);
			}
			else
			{
				//var a = AsyncWaitFor(_client.GetTokenInfoAsync(Token));
				//base.WriteWrappedData(a, KeepSecretWrapper);

				//var r = AsyncWaitFor(_client.ReadSecretAsync($"auth/token/lookup/{Token}"));
				var r = AsyncWaitFor(_session.ReadSecret<TokenInfo>($"auth/token/lookup/{Token}"));
				WriteWrappedData(r, KeepSecretWrapper);
			}
		}

	}
}
