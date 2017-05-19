using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "Policy", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(IEnumerable<string>))]
	[OutputType(typeof(Policy), ParameterSetName = new[] { ForNameParamSet})]
	public class GetPolicy : VaultBaseCmdlet
	{
		public const string ForNameParamSet = "ForName";

		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = ForNameParamSet)]
		public string[] Name
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			if (Name?.Length > 0)
			{
				foreach (var n in Name)
					base.WriteObject(_client.GetPolicyAsync(n).Result);
			}
			else
			{
				base.WriteObject(_client.GetAllPoliciesAsync().Result);
			}
		}
	}
}
