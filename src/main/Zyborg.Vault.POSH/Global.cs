namespace Zyborg.Vault.POSH
{
	public static class Global
	{
		public const string CliVaultAddressEnvName = "VAULT_ADDR";
		public const string CliVaultTokenEnvName = "VAULT_TOKEN";
		public const string CliVaultTokenCacheFile = @"$HOME\.vault-token";

		public const string VaultAddressCacheFile = @"$HOME\.vault-address";
		public const string VaultProfilesDir = @"$HOME\.vault-profiles";
		public const string VaultProfileFileFormat = "{0}.profile";
	}
}
