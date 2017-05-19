using System.Collections.Generic;
using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "TokenCapabilities", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(IEnumerable<string>))]
	public class GetTokenCapabilities : VaultBaseCmdlet
	{
		public const string AccessorParamSet = "Accessor";

		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Path
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1, ParameterSetName = DefaultParamSet)]
		public string Token
		{ get; set; }

		[Parameter(Mandatory = false, Position = 1, ParameterSetName = AccessorParamSet)]
		public string Accessor
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			if (!string.IsNullOrWhiteSpace(Accessor))
				foreach (var p in Path)
					base.WriteObject(_client.GetTokenAccessorCapabilitiesAsync(Accessor, p).Result);
			else if (!string.IsNullOrWhiteSpace(Token))
				foreach (var p in Path)
					base.WriteObject(_client.GetTokenCapabilitiesAsync(Token, p).Result);
			else
				foreach (var p in Path)
					base.WriteObject(_client.GetCallingTokenCapabilitiesAsync(p).Result);
		}
	}
}
