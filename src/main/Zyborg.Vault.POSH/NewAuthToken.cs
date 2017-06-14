using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.Authentication.Models.Token;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Create a new auth token.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'token-create'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command creates a new token that can be used for authentication.
	/// This token will be created as a child of your token.The created token
	/// will inherit your policies, or can be assigned a subset of your policies.
	/// </para>
	/// <para type="description">
	/// A lease can also be associated with the token.If a lease is not associated
	/// with the token, then it cannot be renewed.If a lease is associated with
	/// the token, it will expire after that amount of time unless it is renewed.
	/// </para>
	/// <para type="description">
	/// Metadata associated with the token(specified with "-metadata") is
	/// written to the audit log when the token is used.
	/// </para>
	/// <para type="description">
	/// If a role is specified, the role may override parameters specified here.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.New, "AuthToken", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(WrapInfo), ParameterSetName = new[] { WrapParamSet })]
	[OutputType(typeof(AuthorizationInfo))]
	public class NewAuthToken : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// The token value that clients will use to authenticate with Vault.
		/// </para><para type="description">
		/// If not provided this defaults to a 36 character UUID.
		/// A root token is required to specify the ID of a token.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string Id
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// A display name to associate with this token.
		/// </para><para type="description">
		/// This is a non-security sensitive value used to help
		/// identify created secrets, i.e.prefixes.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string DisplayName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Initial TTL to associate with the token; renewals can extend this value.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string InitialTtl
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// An explicit maximum lifetime for the token. Unlike normal token TTLs,
		/// which can be renewed up until the maximum TTL set on the auth/token
		/// mount or the system configuration file, this lifetime is a hard limit
		/// set on the token itself and cannot be exceeded.
		/// </para><para type="description">
		/// This corresponds to the <c>'explicit-max-ttl'</c> vault CLI parameter.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string MaxTtl
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// TOOD:  THIS PARAMETER IS CURRENTLY NOT SUPPORTED!
		/// </para>
		/// <para type="description">
		/// If specified, the token will be periodic; it will
		/// have no maximum TTL (unless a MaxTtl is
		/// also set) but every renewal will use the given
		/// period. Requires a root/sudo token to use.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string RenewalPeriod
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Whether or not the token is renewable to extend its TTL up to Vault's
		/// configured maximum TTL for tokens.
		/// </para><para type="description">
		/// This defaults to true; set to false to disable renewal of this token.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter Renewable
		{ get; set; } = true;

		/// <summary>
		/// <para type="description">
		/// Metadata to associate with the token.
		/// </para><para type="description">
		/// This shows up in the audit log.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public Hashtable Metadata
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// If specified, the token will have no parent.
		/// </para><para type="description">
		/// This prevents the new token from being revoked with your token.
		/// Requires a root/sudo token to use.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter Orphan
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// If specified, the token will not have the "default"
		/// policy included in its policy set.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter NoDefaultPolicy
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// One ore more policies to associate with this token.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string[] Policy
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The number of times this token can be used until it is
		/// automatically revoked.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public int UseLimit
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// If set, the token will be created against the named role.
		/// </para><para type="description">
		/// The role may override other parameters.  This requires
		/// the client to have permissions on the appropriate
		/// endpoint (auth/token/create/[name]).
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string Role
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Indicates that the response should be wrapped in a
		/// cubbyhole token with the requested TTL.
		/// </para><para type="description">
		/// This is a numeric string with an optional suffix "s", "m", or "h";
		/// if no suffix is specified it will be parsed as seconds.
		/// </para><para type="description">
		/// The unwrapped response can be fetched by calling the command
		/// again but providing the wrapped token ID to the <c>UnwrapToken</c>
		/// parameter.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = WrapParamSet)]
		public string WrapTtl
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
			Dictionary<string, string> md = null;
			if (Metadata != null && Metadata.Count > 0)
			{
				md = new Dictionary<string, string>();
				foreach (var key in Metadata.Keys)
					md[key.ToString()] = Metadata[key].ToString();
			}
			List<string> pol = null;
			if (Policy?.Length > 0)
				pol = new List<string>(Policy);

			var tco = new TokenCreationOptions
			{
				Id = Id,
				DisplayName = DisplayName,
				TimeToLive = InitialTtl,
				ExplicitMaximumTimeToLive = MaxTtl,
				// TODO:
			  //??? = RenewalPeriod,
				Renewable = Renewable,
				Metadata = md,
				CreateAsOrphan = Orphan,
				NoDefaultPolicy = NoDefaultPolicy,
				Policies = pol,
				MaximumUsageCount = UseLimit,
				RoleName = Role,
			};

			var d = AsyncWaitFor(_client.CreateTokenAsync(tco));
			if (!string.IsNullOrEmpty(WrapTtl))
			{
				// TODO:  Is this even possible
				//var w = AsyncWaitFor(_client.WrapResponseDataAsync(d.AuthorizationInfo, WrapTtl));
				//WriteWrappedData(w, KeepSecretWrapper);

				throw new NotSupportedException("wrapping AuthorizationInfo");
			}
			else
			{
				WriteWrappedAuth(d, KeepSecretWrapper);
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
