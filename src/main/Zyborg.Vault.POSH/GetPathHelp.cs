﻿using Newtonsoft.Json;
using System.Management.Automation;
using System.Net.Http;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "PathHelp")]
	[OutputType(typeof(PathHelp))]
	public class GetPathHelp : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Path
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var p in Path)
			{
				var r = _session.MakeVaultApiRequest<PathHelp>($"{p}?help=1",
						HttpMethod.Get).Result;
				base.WriteObject(r);
			}
		}
	}

	public class PathHelp
	{
		[JsonProperty("help")]
		public string Help
		{ get; set; }

		[JsonProperty("see_also")]
		public string SeeAlso
		{ get; set; }
	}
}
