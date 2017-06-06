using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.Backends.System.Models;

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
		[Alias("vs")]
		public object VaultSession
		{ get; set; }

		[Parameter()]
		[Alias("vp")]
		public string VaultProfile
		{ get; set; }

		[Parameter()]
		[Alias("va")]
		public string VaultAddress
		{ get; set; }

		[Parameter()]
		[Alias("vt")]
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
			VaultProfile profile = null;

			_session = VaultSession as VaultSession;
			if (_session == null)
			{
				// First see if user specified a named profile
				if (!string.IsNullOrEmpty(VaultProfile))
				{
					profile = Global.GetVaultProfile(this, VaultProfile);
					if (profile == null)
						throw new FileNotFoundException($"missing user profile [{VaultProfile}]");
				}
				else
				{
					// Then default to possible default profile, may be null
					profile = Global.GetVaultProfile(this, Global.DefaultVaultProfileName);
				}

				var address = ResolveAddress(profile);
				var token = ResolveToken(profile);

				WriteVerbose($"Creating Vault Session with address [{address}]");
				_session = new VaultSession(address, token);
			}

			_client = _session.VaultClient;

			// Make sure TLS1.2 is enabled as that's often a requirement with Vault Server
			if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12))
			{
				WriteVerbose("Enabling TLS 1.2 support, typically required for Vault");
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
			}

			return _client;
		}

		protected string ResolveAddress(VaultProfile profile = null)
		{
			// Resolve Vault Address
			var address = VaultAddress ?? profile?.VaultAddress;

			if (string.IsNullOrWhiteSpace(address))
			{
				WriteVerbose("Trying to resolve Vault address from env");
				address = Environment.GetEnvironmentVariable(Global.CliVaultAddressEnvName);
			}

			var addressCacheFile = base.InvokeCommand.ExpandString(Global.VaultAddressCacheFile);
			if (string.IsNullOrWhiteSpace(address))
			{
				if (File.Exists(addressCacheFile))
				{
					WriteVerbose($"Trying to load Vault address from user address cache file [{addressCacheFile}]");
					address = File.ReadAllText(addressCacheFile)?.Trim();
				}
			}
			else if (base.MyInvocation.BoundParameters.ContainsKey(nameof(VaultAddress)))
			{
				WriteVerbose($"Saving manually-specified Vault address to user cache file [{addressCacheFile}]");
				File.WriteAllText(addressCacheFile, address);
			}

			// Absolute last resort -- assume the default address for a local DEV address
			if (string.IsNullOrWhiteSpace(address))
			{
				WriteVerbose("Defaulting to local dev Vault address");
				address = "http://127.0.0.1:8200";
			}

			return address;
		}

		protected string ResolveToken(VaultProfile profile = null)
		{
			// Resolve Vault Token
			var token = VaultToken ?? profile?.VaultToken;

			if (string.IsNullOrEmpty(token) &&
					!base.MyInvocation.BoundParameters.ContainsKey(nameof(VaultToken)))
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

		protected void AsyncWait(Task t)
		{
			t.Wait();
		}

		protected T AsyncWaitFor<T>(Task<T> t)
		{
			try
			{
				return t.Result;
			}
			catch (AggregateException ex)
			{
				ex = ex.Flatten();
				if (ex.InnerExceptions.Count < 2)
					throw ex.InnerException;
				else
					throw ex;
			}
		}

		protected void WriteAsyncResult<T>(Task<T> t)
		{
			base.WriteObject(AsyncWaitFor(t));
		}

		protected void WriteWrappedData<T>(Secret<T> wrapped, bool keepSecretWrapper)
		{
			if (keepSecretWrapper)
				base.WriteObject(wrapped);
			else if (wrapped != null)
				base.WriteObject(wrapped.Data);
		}

		protected void WriteWrappedAuth<T>(Secret<T> wrapped, bool keepSecretWrapper)
		{
			if (keepSecretWrapper)
				base.WriteObject(wrapped);
			else if (wrapped != null)
				base.WriteObject(wrapped.AuthorizationInfo);
		}

		protected void WriteWrappedEnumerableData<T>(Secret<T> wrapped, bool keepSecretWrapper)
			where T : IEnumerable
		{
			if (keepSecretWrapper)
				base.WriteObject(wrapped);
			else if (wrapped != null)
				foreach (var x in wrapped.Data)
					base.WriteObject(x);
		}

		protected void WriteWrapInfo<T>(Secret<T> wrapped, bool keepSecretWrapper)
		{
			if (keepSecretWrapper)
				base.WriteObject(wrapped);
			else if (wrapped != null)
				base.WriteObject(wrapped.WrappedInformation);
		}
	}
}
