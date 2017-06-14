using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Renew an auth token, extending the amount of time it can be used.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'token-renew'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// You can either specify a specific token ID or indicate to target
	/// auth token of the client caller.
	/// </para><para type="description">
	/// This command is similar to the command <c>'Update-HCVaultSecretLease'</c>
	/// which is used for renewing secret leases.
	/// </para><para type="description">
	/// An optional increment can be given to request a certain number of seconds to
	/// increment the lease.  This request is advisory; Vault may not adhere to it at all.
	/// </para>
	/// <para type="link">Update-HCVaultSecretLease</para>
	/// </remarks>
	[Cmdlet(VerbsData.Update, "AuthToken", DefaultParameterSetName = TokenParamSet)]
	public class UpdateAuthToken : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string TokenParamSet = "Token";
		public const string SelfParamSet = "Self";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
		
		/// <summary>
		/// <para type="description">
		/// The token ID to be renewed.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = TokenParamSet)]
		public string Token
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specify this switch parameter to indicate the token used by the
		/// client caller.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = SelfParamSet)]
		public SwitchParameter Self
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The desired increment for the renewal.
		/// </para><para type="description">
		/// If not supplied, Vault will use the default TTL.  If supplied,
		/// it may still be ignored.  This can be submitted as an integer
		/// number of seconds or a string duration (e.g. "72h").
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public int? Increment
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
