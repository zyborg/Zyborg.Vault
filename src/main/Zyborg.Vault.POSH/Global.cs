using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// Defines common constants and utility routines.
	/// </summary>
	public static class Global
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string CliVaultAddressEnvName = "VAULT_ADDR";
		public const string CliVaultTokenEnvName = "VAULT_TOKEN";
		public const string CliVaultTokenCacheFile = @"$HOME\.vault-token";

		public const string VaultAddressCacheFile = @"$HOME\.vault-address";
		public const string VaultProfilesDir = @"$HOME\.vault-profiles";
		public const string VaultProfileFileFormat = "{0}.profile";
		public const string DefaultVaultProfileName = "default";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public static IEnumerable<string> GetVaultProfileNames(PSCmdlet ctx)
		{
			var profileDir = ctx.InvokeCommand.ExpandString(Global.VaultProfilesDir);
			ctx.WriteVerbose($"Resolved user profiles root directory [{profileDir}]");

			var dir = new DirectoryInfo(profileDir);
			if (!dir.Exists)
			{
				ctx.WriteVerbose("Missing user profiles root directory");
				yield break;
			}

			var wildcard = string.Format(Global.VaultProfileFileFormat, "*");
			var regex = string.Format(Global.VaultProfileFileFormat.Replace(".", "\\."), "(.+)");

			foreach (var f in dir.GetFiles(wildcard))
			{
				var m = Regex.Match(f.Name, regex);
				if (m.Success)
				{
					yield return m.Groups[1].Value;
				}
			}
		}

		public static VaultProfile GetVaultProfile(PSCmdlet ctx, string name)
		{
			var profileDir = ctx.InvokeCommand.ExpandString(Global.VaultProfilesDir);
			ctx.WriteVerbose($"Resolved user profiles root directory [{profileDir}]");

			if (!Directory.Exists(profileDir))
			{
				ctx.WriteVerbose("Missing user profiles root directory");
				return null;
			}

			var profileFile = Path.Combine(profileDir,
					string.Format(Global.VaultProfileFileFormat, name));
			ctx.WriteVerbose($"Resolved user profile file [{profileFile}]");

			if (File.Exists(profileFile))
			{
				return JsonConvert.DeserializeObject<VaultProfile>(
						File.ReadAllText(profileFile));
			}

			return null;
		}

		public static void SetVaultProfile(PSCmdlet ctx, string name, bool Remove = false,
				bool force = false, string vaultAddress = null, string vaultToken = null,
				string label = null)
		{
			var profileDir = ctx.InvokeCommand.ExpandString(Global.VaultProfilesDir);
			ctx.WriteVerbose($"Resolved user profiles root directory [{profileDir}]");

			var profileFile = Path.Combine(profileDir,
					string.Format(Global.VaultProfileFileFormat, name));
			ctx.WriteVerbose($"Resolved user profile file [{profileFile}]");

			if (Remove)
			{
				if (File.Exists(profileFile))
				{
					ctx.WriteVerbose("Removing profile file");
					File.Delete(profileFile);
				}
			}
			else
			{
				if (!Directory.Exists(profileDir))
				{
					// TODO: need to provide a default Directory ACL to
					// protect the profiles directory

					ctx.WriteVerbose("Creating user profiles root directory");
					Directory.CreateDirectory(profileDir);
				}

				if (File.Exists(profileFile) && !force)
					throw new Exception("Existing profile found, use -Force to overwrite");

				var vp = new VaultProfile
				{
					Label = label,
					VaultAddress = vaultAddress,
					VaultToken = vaultToken,
				};

				ctx.WriteVerbose("Saving VaultProfile to file");
				File.WriteAllText(profileFile, JsonConvert.SerializeObject(vp));
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
