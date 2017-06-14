using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Delete a policy with the given name.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'policy-delete'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Once the policy is deleted, all users associated with the policy will
	/// be affected immediately.  When a user is associated with a policy that
	/// doesn't exist, it is identical to not being associated with that policy.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Remove, "Policy")]
	public class RemovePolicy : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// One or policie names to delete.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Name
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var n in Name)
			{
				AsyncWait(_client.DeletePolicyAsync(n));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
