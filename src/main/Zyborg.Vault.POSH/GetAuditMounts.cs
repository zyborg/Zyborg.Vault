using System.Management.Automation;
using VaultSharp.Backends.Audit.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// List the enabled audit backends.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>audit-list</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// The output lists the mounted audit providers and the options for those
	/// providers.  The options may contain sensitive information, and therefore
	/// only a root Vault user can view this.
	/// </para><para type="description">
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Get, "AuditMounts")]
	[OutputType(typeof(AuditBackend))]
	public class GetAuditMounts : VaultBaseCmdlet
	{
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

		protected override void ProcessRecord()
		{
			var r = AsyncWaitFor(_client.GetAllEnabledAuditBackendsAsync());
			WriteWrappedEnumerableData(r, KeepSecretWrapper);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
