using System.Collections.Generic;
using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Renew the lease on a secret, extending the time that it can
	/// be used before it is revoked by Vault.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'renew'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Every secret in Vault has a lease associated with it. If the user of
	/// the secret wants to use it longer than the lease, then it must be
	/// renewed.  Renewing the lease will not change the contents of the secret.
	/// </para><para type="description">
	/// To renew a secret, run this command with the lease ID returned when it
	/// was read.  Optionally, request a specific increment in seconds. Vault
	/// is not required to honor this request.
	/// </para>
	/// <para type="link">Update-VltAuthToken</para>
	/// </remarks>
	[Cmdlet(VerbsData.Update, "SecretLease")]
	[OutputType(typeof(Dictionary<string, object>))]
	public class UpdateSecretLease : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// The secret lease ID to renew.
		/// </para><para type="description">
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0)]
		public string LeaseId
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
			var r = AsyncWaitFor(_client.RenewSecretAsync(LeaseId, Increment));
			WriteWrappedData(r, KeepSecretWrapper);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
