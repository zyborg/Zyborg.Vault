using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsData.Update, "SecretLease")]
	public class UpdateSecretLease : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0)]
		public string LeaseId
		{ get; set; }

		[Parameter(Mandatory = false)]
		public int? Increment
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void EndProcessing()
		{
			AsyncWait(_client.RenewSecretAsync(LeaseId, Increment));
		}
	}
}
