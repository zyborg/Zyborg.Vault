using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Backends.Authentication.Models;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	[Cmdlet(VerbsCommon.New, "Auth", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(AuthorizationInfo))]
	public class NewAuth : VaultBaseCmdlet
	{
		public const string SaveAsParamSet = "SaveAs";

		[Parameter(Mandatory = false)]
		public string AuthMethod
		{ get; set; } = "token";

		[Parameter(Mandatory = false)]
		public string MountName
		{ get; set; }

		[Parameter(Mandatory = false)]
		public string PathData
		{ get; set; }

		[Parameter(Mandatory = false)]
		public Hashtable AuthData
		{ get; set; }

		[Parameter(Mandatory = false)]
		public SwitchParameter NoVerify
		{ get; set; }

		[Parameter(Mandatory = false, ParameterSetName = DefaultParamSet)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		[Parameter(Mandatory = false, ParameterSetName = SaveAsParamSet)]
		public string SaveAsProfile
		{ get; set; }

		[Parameter(Mandatory = false, ParameterSetName = SaveAsParamSet)]
		public SwitchParameter Force
		{ get; set; }

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			var authMount = MountName ?? AuthMethod;
			var resPath = $"auth/{authMount}/login";
			if (!string.IsNullOrEmpty(PathData))
				resPath += $"/{PathData.TrimStart('/')}";

			var r = AsyncWaitFor(_session.MakeVaultApiRequest<Secret<Dictionary<string, object>>>(
					resPath, HttpMethod.Post, AuthData));

			if (!string.IsNullOrEmpty(SaveAsProfile))
			{
				var token = r.AuthorizationInfo.ClientToken;
				if (string.IsNullOrEmpty(token))
					throw new Exception("empty token");

				Global.SetVaultProfile(this, SaveAsProfile, force: Force,
						vaultAddress: _session.VaultAddress, vaultToken: token);
			}
			else
			{
				WriteWrappedAuth(r, KeepSecretWrapper);
			}
		}
	}
}
