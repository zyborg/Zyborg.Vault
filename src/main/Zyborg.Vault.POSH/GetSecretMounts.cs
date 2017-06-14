using System.Management.Automation;
using VaultSharp.Backends.Secret.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Outputs information about the mounted secret providers.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>mounts</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This command lists the mounted secret providers, their mount points,
	/// the configured TTLs, and a human-friendly description of the mount point.
	/// </para><para type="description">
	/// A TTL of 'system' indicates that the system default is being used.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Get, "SecretMounts")]
	[OutputType(typeof(SecretBackend))]
	public class GetSecretMounts : VaultBaseCmdlet
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
			base.ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var r = AsyncWaitFor(_client.GetAllMountedSecretBackendsAsync());
			WriteWrappedEnumerableData(r, KeepSecretWrapper);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
