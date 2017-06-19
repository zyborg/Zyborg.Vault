using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Delete data (secrets or configuration) from Vault.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'delete'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Delete sends a delete operation request to the given path.
	/// </para><para type="description">
	/// The behavior of the delete is determined by the backend at the given
	/// path.  For example, deleting "aws/policy/ops" will delete the "ops"
	/// policy for the AWS backend.
	/// </para><para type="description">
	/// Use <c>'Get-VltPathHelp'</c> for more details on whether delete is
	/// supported for a path and what the behavior is.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Remove, "Data")]
	public class RemoveData : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// One ore more paths to delete data from.
		/// </para><para type="description">
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Path
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var p in Path)
			{
				AsyncWait(_client.DeleteSecretAsync(p));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
