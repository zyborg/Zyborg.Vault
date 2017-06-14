using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Revoke one or more auth tokens.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'token-revoke'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command revokes auth tokens.
	/// </para><para type="description">
	/// The related command <c>'Revoke-HCVaultSecret'</c> is used to revoke secrets.
	/// </para><para type="description">
	/// By default a token is revoked along with all of its children.
	/// </para><para type="description">
	/// With the <c>'-OrphanChildren'</c> switch parameter, only the token will be revoked
	/// and all of its children will be orphaned.
	/// </para><para type="description">
	/// With the <c>'-PathPrefix'</c> parameter, tokens created from the given auth path
	/// prefix will be deleted, along with all their children.  The path cannot specify
	/// token values or parts of token values.
	/// </para>
	/// <para type="link">Revoke-HCVaultSecret</para>
	/// </remarks>
	[Cmdlet(VerbsSecurity.Revoke, "AuthToken", DefaultParameterSetName = TokenParamSet)]
	public class RevokeAuthToken : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string TokenParamSet = "Token";
		public const string AccessorParamSet = "Accessor";
		public const string SelfParamSet = "Self";
		public const string PathPrefixParamSet = "PathPrefix";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The token ID to revoke.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = TokenParamSet)]
		public string Token
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The accessor of the token to revoke.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = AccessorParamSet)]
		public string Accessor
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// This switch parameter indicates the token of the currently authenticated
		/// caller should be revoked.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = SelfParamSet)]
		public SwitchParameter Self
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Tokens created from the given auth path prefix will be revoked along
		/// with all their children.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PathPrefixParamSet)]
		public string PathPrefix
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// This switch parameter indicates only the specific token will be
		/// revoked and all of its children will be orphaned.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = TokenParamSet)]
		public SwitchParameter OrphanChildren
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
