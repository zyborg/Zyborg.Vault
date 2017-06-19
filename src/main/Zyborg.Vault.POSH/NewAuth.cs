using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.Http;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Authenticate with Vault against an auth provider using argument
	/// auth data.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>auth</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// By default, the <c>AuthMethod</c> is assumed to be token.
	/// If not supplied via the <c>AuthData</c> parameter, a prompt will be issued
	/// to collect the token ID used to authenticate against the token auth provider.
	/// </para><para type="description">
	/// </para>
	/// <para type="link">Set-VltProfile</para>
	/// <para type="link">Get-VltProfile</para>
	/// </remarks>
	/// <example>
	/// <para>
	/// An example of authenticating to the 'userpass' auth provider at the
	/// default mount point.
	/// </para><code>
	/// ##
	///   
	///   $creds = Get-Credential ## Interactively and securely prompt for auth creds
	///   New-VltAuth -AuthMethod userpass -PathData $creds.Username `
	///       -AuthData @{ password = $creds.GetNetworkCredential().Password }
	/// </code>
	/// </example>
	/// <example>
	/// <para>
	/// An example of authenticating to the <c>okta</c> auth provider at the
	/// non-default mount point 'okta1'.
	/// </para><code>
	/// ##
	/// 
	///   $creds = Get-Credential ## Interactively and securely prompt for auth creds
	///   New-VltAuth -AuthMethod okta -MountName okta1 -PathData $creds.Username `
	///       -AuthData @{ password = $creds.GetNetworkCredential().Password }
	/// </code>
	/// </example>
	[Cmdlet(VerbsCommon.New, "Auth", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(AuthorizationInfo))]
	public class NewAuth : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string SaveAsParamSet = "SaveAs";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// Selects the authentication method to be used, such as userpass,
		/// GitHub, or TLS certificates.
	    /// </para><para type="description">
		/// Each method corresponds to an associated auth provider which defines
		/// its own set of authentication parameters which are provided via the
		/// <c>AuthData</c> parameter.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string AuthMethod
		{ get; set; } = "token";

		/// <summary>
		/// <para type="description">
		/// The path at which the auth provider is mounted. If an auth provider is mounted at
		/// multiple paths, this option can be used to authenticate against a specific path
		/// associated with a specific provider instance (i.e. with a distinct configuration).
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string MountName
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Defines the sub-path under the mount to authenticate against.
		/// </para><para type="description">
		/// By default authentication is targeted against the <c>login</c>
		/// sub-path under the mount point of an auth provider instance.
		/// This parameter allows you to define an additional sub-path to
		/// the default.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public string PathData
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specifies arbitrary authentication parameters as key-value pairs.
		/// </para><para type="description">
		/// Authentication parameters are specific to each authentication
		/// method, aka auth provider, which define various required or
		/// optional parameter key names.  Refer to the documentation of
		/// specific auth providers for more details about specific auth
		/// data key names.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public Hashtable AuthData
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Do not verify the token after creation; avoids a use count decrement.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter NoVerify
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// When specified, the returned result maintains the meta data wrapper
		/// for the secret result.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Saves the result of the new auth token request to a Vault
		/// connection profile with the argument name.
		/// </para><para type="description">
		/// If the profile name already exists, use the <c>Force</c> switch
		/// parameter to overwrite it, otherwise an error will be raised
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = SaveAsParamSet)]
		public string SaveAsProfile
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// When saving the auth token to a profile with the <c>SaveAsProfile</c>
		/// parameter, use this flag to overwrite an existing profile with the
		/// same name, otherwise an error will be raised.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = SaveAsParamSet)]
		public SwitchParameter Force
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var authMount = MountName ?? AuthMethod;
			var resPath = $"auth/{authMount}/login";
			if (!string.IsNullOrEmpty(PathData))
				resPath += $"/{PathData.TrimStart('/')}";

			var r = AsyncWaitFor(_session.MakeVaultApiRequest<Secret<Dictionary<string, object>>>(
					resPath, HttpMethod.Post, AuthData));

			if (!string.IsNullOrEmpty(SaveAsProfile))
			{
				var token = r.AuthorizationInfo.ClientToken;
				if (string.IsNullOrEmpty(token))
					throw new Exception("empty token");

				Global.SetVaultProfile(this, SaveAsProfile, force: Force,
						vaultAddress: _session.VaultAddress, vaultToken: token);
			}
			else
			{
				WriteWrappedAuth(r, KeepSecretWrapper);
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
