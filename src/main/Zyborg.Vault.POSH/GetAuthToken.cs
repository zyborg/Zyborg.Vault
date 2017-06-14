using System.Management.Automation;
using VaultSharp.Backends.System.Models;
using Zyborg.Vault.POSH.Model;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Retrieves information about tokens.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>token-lookup</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// You can use this command to retrieve a list of tokens, or
	/// to get information about specific tokens, either by their
	/// ID (token value) or the associated token 'accessor'.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Get, "AuthToken", DefaultParameterSetName = TokenParamSet)]
	public class GetAuthToken : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string TokenParamSet = "Token";
		public const string AccessorParamSet = "Accessor";
		public const string SelfParamSet = "Self";
		public const string ListParamSet = "List";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The token ID to lookup.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = TokenParamSet)]
		public string Token
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The accessor of a token to lookup.
		/// </para><para type="description">
		/// Note that the response will not contain the token ID.
		/// Accessor is only meant for looking up the token properties.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = AccessorParamSet)]
		public string Accessor
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Indicates to do a lookup of the currently authenticated token (lookup-self).
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = SelfParamSet)]
		public SwitchParameter Self
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Lists the accessors of existing tokens.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = ListParamSet)]
		public SwitchParameter ListAccessors
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// When specified, the returned result maintains the meta data wrapper
		/// for the secret result.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
