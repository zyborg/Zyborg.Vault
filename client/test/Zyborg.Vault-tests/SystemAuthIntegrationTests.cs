using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Ext.Token;
using Zyborg.Vault.Ext.Userpass;

namespace Zyborg.Vault
{
    public class SystemAuthIntegrationTests
    {
        public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";


        [Fact]
        public async void ConfirmBaseAuthAreEnabled()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;
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