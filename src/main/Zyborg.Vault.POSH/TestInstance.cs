using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsDiagnostic.Test, "Instance")]
	public class TestInstance : VaultBaseCmdlet
	{
		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			base.WriteAsyncResult(_client.GetInitializationStatusAsync());
		}
	}
}
