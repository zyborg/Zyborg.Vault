﻿using System.Collections.Generic;
using System.Management.Automation;
using System.Net.Http;
using VaultSharp.Backends.Secret.Models;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.Get, "DataList", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(WrapInfo), ParameterSetName = new[] { WrapParamSet })]
	[OutputType(typeof(ListInfo))]
	public class GetDataList : VaultBaseCmdlet
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
					var r = _client.UnwrapWrappedResponseDataAsync<ListInfo>(t).Result;
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
					// Wrap
					if (!string.IsNullOrEmpty(WrapTtl))
					{
						var r = _session.MakeVaultApiRequest<Secret<Dictionary<string, object>>>($"{p}?list=true",
								HttpMethod.Get).Result;

						var w = _client.WrapResponseDataAsync(r.Data, WrapTtl).Result;
						if (KeepSecretWrapper)
							base.WriteObject(w);
						else
							base.WriteObject(w.WrappedInformation);
					}
					// Default
					else
					{
						var r = _session.MakeVaultApiRequest<Secret<ListInfo>>($"{p}?list=true",
								HttpMethod.Get).Result;

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
