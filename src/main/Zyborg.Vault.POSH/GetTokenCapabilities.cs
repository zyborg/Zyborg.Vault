using System.Collections.Generic;
using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Fetch the capabilities of a token on a given path.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>capabilities</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// The capabilities can be fetched for a specific token ID, a
	/// token accessor or, if neither is provided, for the current
	/// user client token.
	/// </para><para type="description">
	/// If a token does not have any capability on a given path, or if any of the policies
	/// belonging to the token explicitly have["deny"] capability, or if the argument path
	/// is invalid, this command will respond with a ["deny"].
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Get, "TokenCapabilities", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(IEnumerable<string>))]
	public class GetTokenCapabilities : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string AccessorParamSet = "Accessor";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// One or more paths to get capabilities for.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Path
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The token ID to inquire capabilities for.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 1, ParameterSetName = DefaultParamSet)]
		public string Token
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The token accessor to inquire capabilities for.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 1, ParameterSetName = AccessorParamSet)]
		public string Accessor
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			if (!string.IsNullOrWhiteSpace(Accessor))
				foreach (var p in Path)
					WriteAsyncResult(_client.GetTokenAccessorCapabilitiesAsync(Accessor, p));
			else if (!string.IsNullOrWhiteSpace(Token))
				foreach (var p in Path)
					WriteAsyncResult(_client.GetTokenCapabilitiesAsync(Token, p));
			else
				foreach (var p in Path)
					WriteAsyncResult(_client.GetCallingTokenCapabilitiesAsync(p));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
