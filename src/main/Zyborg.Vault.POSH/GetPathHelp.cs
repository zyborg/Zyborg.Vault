using System.Management.Automation;
using System.Net.Http;
using Zyborg.Vault.POSH.Model;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Look up the help for a path.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>path-help</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// All endpoints in Vault from system paths, secret paths, and credential
	/// providers provide built-in help.
	/// This command looks up and outputs that help.
	/// </para><para type="description">
	/// The command requires that the vault be unsealed, because otherwise
	/// the mount points of the backends are unknown.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Get, "PathHelp")]
	[OutputType(typeof(PathHelp))]
	public class GetPathHelp : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// One or more paths to get help for.
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
				WriteAsyncResult(_session.MakeVaultApiRequest<PathHelp>($"{p}?help=1",
						HttpMethod.Get));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
