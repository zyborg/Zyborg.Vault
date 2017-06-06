using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Backends.Authentication.Models.Token;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">Create a new auth token.</para>
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
		[Parameter(Mandatory = false)]
		public string Id
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string DisplayName
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string InitialTtl
		{ get; set; }

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

		[Parameter(Mandatory = false)]
		public SwitchParameter Renewable
		{ get; set; } = true;

		[Parameter(Mandatory = false)]
		public Hashtable Metadata
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter Orphan
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter NoDefaultPolicy
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string[] Policy
		{ get; set; }

		[Parameter(Mandatory = false)]
		public int UseLimit
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string Role
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = WrapParamSet)]
		public string WrapTtl
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
	}
}
