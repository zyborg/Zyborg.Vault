using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// List the policies that are available or read a single policy.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>policies</c> command.
	/// This command lists the policies that are written to the Vault server.
	/// If a name of a policy is specified, that policy is outputted.
	/// </para>
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "Policy", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(IEnumerable<string>))]
	[OutputType(typeof(Policy), ParameterSetName = new[] { ForNameParamSet})]
	public class GetPolicy : VaultBaseCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string ForNameParamSet = "ForName";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The name of one or more policies to retrieve.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = ForNameParamSet)]
		public string[] Name
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			if (Name?.Length > 0)
			{
				foreach (var n in Name)
					WriteAsyncResult(_client.GetPolicyAsync(n));
			}
			else
			{
				WriteAsyncResult(_client.GetAllPoliciesAsync());
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
