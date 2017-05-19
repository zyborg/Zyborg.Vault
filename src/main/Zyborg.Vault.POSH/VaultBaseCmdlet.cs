using System;
using System.IO;
using System.Management.Automation;
using System.Net;
using VaultSharp;

// When troubleshooting connections over SSL in Fiddler, this might help:
//    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

namespace Zyborg.Vault.POSH
{
	public class VaultBaseCmdlet : PSCmdlet
	{
		public const string DefaultParamSet = "Default";
		public const string WrapParamSet = "Wrap";
		public const string UnwrapParamSet = "Unwrap";

		protected VaultSession _session;

		/// <summary>
		/// Caches the result of the most recent invocation to <see cref="ResolveVaultClient"/>.
		/// </summary>
		protected IVaultClient _client;

		[Parameter()]
		public object VaultSession
		{ get; set; }

		[Parameter()]
		public string VaultProfile
		{ get; set; }

		[Parameter()]
		public string VaultServer
		{ get; set; }

		[Parameter()]
		public string VaultToken
		{ get; set; }


		/// <summary>
		/// Resolves a Vault Client instance based on the common parameters passed into
		/// this cmdlet instance, the current environment (variables), and the context
		/// of the invocation, such as user-specific Vault CLI cache and configuration
		/// files, and PowerShell configuration, such as profiles.
		/// </summary>
		/// <returns></returns>
		protected IVaultClient ResolveVaultClient()
		{
			_session = VaultSession as VaultSession;
			if (_session == null)
			{
				//if (!string.IsNullOrEmpty(VaultProfile))
				//{
				//	var 
				//}

				var server = ResolveServer();
				var token = ResolveToken();

				WriteVerbose($"Creating Vault Session with server address [{server}]");
				_session = new VaultSession(server, token);
			}

			_client = _session.VaultClient;

			// Make sure TLS1.2 is enabled as that's often a requirement with Vault Server
			if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12))
			{
				WriteVerbose("Enabling TLS 1.2 support, typically required for Vault Server");
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
			}

			return _client;
		}

		protected string ResolveServer()
		{
			// Resolve Vault Server
			var server = VaultServer;

			if (string.IsNullOrWhiteSpace(server))
			{
				WriteVerbose("Trying to resolve Vault Server address from env");
				server = Environment.GetEnvironmentVariable(Global.CliVaultServerAddressEnvName);
			}
			var serverCacheFile = base.InvokeCommand.ExpandString(Global.VaultServerCacheFile);
			if (string.IsNullOrWhiteSpace(server))
			{
				if (File.Exists(serverCacheFile))
				{
					WriteVerbose($"Trying to load Vault Server address from user server cache file [{serverCacheFile}]");
					server = File.ReadAllText(serverCacheFile)?.Trim();
				}
			}
			else
			{
				WriteVerbose($"Saving resolved Vault Server address to user server cache file [{serverCacheFile}]");
				File.WriteAllText(serverCacheFile, server);
			}

			// Absolute last resort -- assume the default address for a local DEV server
			if (string.IsNullOrWhiteSpace(server))
			{
				WriteVerbose("Defaulting to local dev Vault Server address");
				server = "http://127.0.0.1:8200";
			}

			return server;
		}

		protected string ResolveToken()
		{
			// Resolve Vault Token
			var token = VaultToken;

			if (!base.MyInvocation.BoundParameters.ContainsKey(nameof(VaultToken)))
			{
				WriteVerbose("Trying to resolve Vault Token from env");
				token = Environment.GetEnvironmentVariable(Global.CliVaultTokenEnvName);
				if (string.IsNullOrWhiteSpace(token))
				{
					var cliTokenCacheFile = base.InvokeCommand.ExpandString(Global.CliVaultTokenCacheFile);
					if (File.Exists(cliTokenCacheFile))
					{
						WriteVerbose($"Trying to load Vault Token from CLI user token cache file [{cliTokenCacheFile}]");
						token = File.ReadAllText(cliTokenCacheFile)?.Trim();
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(token))
				WriteVerbose("Using Vault Token");

			return token;
		}
	}
}
