using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Ext.Token;
using Zyborg.Vault.Ext.Userpass;

namespace Zyborg.Vault
{
    public class SystemAuthIntegrationTests
    {
        public const string TestVaultAddress = TestConfig.TestVaultAddress;

        public static readonly string TestRootToken = TestConfig.RootTokens[TestVaultAddress];


        [Fact]
        public async void ConfirmBaseAuthAreEnabled()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;
                var auths = await client.ListAuthBackendsAsync();

                var tokenMount = $"{TokenAuthExtensions.DefaultMountName}/";
                var userpassMount = $"{UserpassAuthExtensions.DefaultMountName}/";

                Assert.Contains(tokenMount, auths.Data.Keys);
                Assert.Contains(userpassMount, auths.Data.Keys);

                Assert.Equal(TokenAuthExtensions.AuthTypeName, auths.Data[tokenMount].Type);
                Assert.Equal(UserpassAuthExtensions.AuthTypeName, auths.Data[userpassMount].Type);
            }
        }
    }
}