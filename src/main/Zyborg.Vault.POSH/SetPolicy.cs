using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Set, "Policy")]
	[OutputType(typeof(Secret<Dictionary<string, object>>))]
	public class SetPolicy : VaultBaseCmdlet
	{
		// Single-Path, Multiple Key/Value sets
		[Parameter(Mandatory = true, Position = 0,
				ValueFromPipelineByPropertyName = true)]
		public string Name
		{ get; set; }

		[Parameter(Mandatory = true, Position = 1,
				ValueFromPipelineByPropertyName = true)]
		public string Rules
		{ get; set; }

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
	}
}
