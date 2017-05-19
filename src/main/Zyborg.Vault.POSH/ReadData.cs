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
					var r = _client.UnwrapWrappedResponseDataAsync(t).Result;
					if (KeepSecretWrapper)
						base.WriteObject(r);
					else
						base.WriteObject(r.Data);
				}
			}
			else
			{
				foreach (var p in Path)
				{
					var r = _client.ReadSecretAsync(p).Result;

					// Wrap
					if (!string.IsNullOrEmpty(WrapTtl))
					{
						var w = _client.WrapResponseDataAsync(r.Data, WrapTtl).Result;
						if (KeepSecretWrapper)
							base.WriteObject(w);
						else
							base.WriteObject(w.WrappedInformation);
					}
					// Default
					else
					{
						if (KeepSecretWrapper)
							base.WriteObject(r);
						else
							base.WriteObject(r.Data);
					}
				}
			}
		}
	}
}
