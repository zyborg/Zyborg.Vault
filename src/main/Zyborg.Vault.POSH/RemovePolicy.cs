using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Remove, "Policy")]
	public class RemovePolicy : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
		public string[] Name
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			foreach (var n in Name)
			{
				AsyncWait(_client.DeletePolicyAsync(n));
			}
		}
	}
}
