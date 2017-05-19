namespace Zyborg.Vault.POSH
{
	public static class Global
	{
		public const string CliVaultServerAddressEnvName = "VAULT_SERVER";
		public const string CliVaultTokenEnvName = "VAULT_TOKEN";
		public const string CliVaultTokenCacheFile = @"$HOME\.vault-token";

		public const string VaultServerCacheFile = @"$HOME\.vault-server";
		public const string VaultProfilesDir = @"$HOME\.vault-profiles";
		public const string VaultProfileFileFormat = "{0}.profile";
	}
}
