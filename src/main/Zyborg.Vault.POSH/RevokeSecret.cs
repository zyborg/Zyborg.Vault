using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Revoke a secret by its lease ID.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'revoke'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command revokes a secret by its lease ID that was returned with it.
	/// Once the key is revoked, it is no longer valid.
	/// </para><para type="description">
	/// The related command <c>'Revoke-VltAuthToken'</c> is used to revoke auth tokens.
	/// </para>
	/// <para type="link">Revoke-VltSecret</para>
	/// </remarks>
	[Cmdlet(VerbsSecurity.Revoke, "Secret", DefaultParameterSetName = LeaseIdParamSet)]
	public class RevokeSecret : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string LeaseIdParamSet = "Token";
		public const string PathPrefixParamSet = "PathPrefix";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The lease ID of a secret to be revoked.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = LeaseIdParamSet)]
		public string LeaseId
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Revoke all secrets with the matching prefix.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PathPrefixParamSet)]
		public string PathPrefix
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Delete the lease even if the actual revocation operation fails.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PathPrefixParamSet)]
		public SwitchParameter Force
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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
