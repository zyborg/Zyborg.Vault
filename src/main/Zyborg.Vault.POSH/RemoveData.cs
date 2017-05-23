using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Remove, "Data")]
	public class RemoveData : VaultBaseCmdlet
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
				AsyncWait(_client.DeleteSecretAsync(p));
			}
		}
	}
}
