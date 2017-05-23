using System;
using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommunications.Read, "Data", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(WrapInfo), ParameterSetName = new[] { WrapParamSet })]
	[OutputType(typeof(Dictionary<string, object>))]
	public class ReadData : VaultBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = DefaultParamSet)]
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = WrapParamSet)]
		public string[] Path
		{ get; set; }

		[Parameter(Mandatory = true, ParameterSetName = WrapParamSet)]
		public string WrapTtl
		{ get; set; }

		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = UnwrapParamSet)]
		public string[] UnwrapToken
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			if (ParameterSetName == UnwrapParamSet)
			{
				foreach (var t in UnwrapToken)
				{
					var r = AsyncWaitFor(_client.UnwrapWrappedResponseDataAsync(t));
					WriteWrappedData(r, KeepSecretWrapper);
				}
			}
			else
			{
				foreach (var p in Path)
				{
					var r = AsyncWaitFor(_client.ReadSecretAsync(p));

					// Wrap
					if (!string.IsNullOrEmpty(WrapTtl))
					{
						var w = AsyncWaitFor(_client.WrapResponseDataAsync(r.Data, WrapTtl));
						WriteWrappedData(w, KeepSecretWrapper);
					}
					// Default
					else
					{
						WriteWrappedData(r, KeepSecretWrapper);
					}
				}
			}
		}
	}
}
