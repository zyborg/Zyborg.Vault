using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Create or update a policy with the given rules.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'policy-write'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// If updating an existing policy, the entire contents of the
	/// existing policy rules definitions will be replaced.
	/// </para><para type="description">
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Set, "Policy")]
	[OutputType(typeof(Secret<Dictionary<string, object>>))]
	public class SetPolicy : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// The name of the policy to create or update.
		/// </para>
		/// </summary>
		// Single-Path, Multiple Key/Value sets
		[Parameter(Mandatory = true, Position = 0,
				ValueFromPipelineByPropertyName = true)]
		public string Name
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The rules content to write to the policy definition.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 1,
				ValueFromPipelineByPropertyName = true)]
		public string Rules
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			base.ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var p = new Policy
			{
				Name = Name,
				Rules = Rules,
			};
			AsyncWait(_client.WritePolicyAsync(p));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
