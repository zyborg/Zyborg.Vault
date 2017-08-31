using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Ext.Userpass;
using Zyborg.Vault.Ext.GenericSecret;
using System.Threading;

namespace Zyborg.Vault_tests
{
    public class SystemLeasesIntegrationTests
    {
         public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";


        [Fact]
        public async void TestLeases()
        {
            // TODO: break out this scenario into multiple tests

            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;
                
                var rootLeases = await client.ListLeasesAsync();
                Assert.NotNull(rootLeases?.Data?.Keys);
                Assert.NotEmpty(rootLeases.Data.Keys);

                await client.CreateUserpassUserAsync("foouser", "barpassword");
                await client.LoginUserpassAsync("foouser", "barpassword");

                var authLeases = await client.ListLeasesAsync("auth/");
                Assert.NotNull(authLeases?.Data?.Keys);
                Assert.NotEmpty(authLeases.Data.Keys);
                Assert.Contains("userpass/", authLeases.Data.Keys);

                var authUserpassLeases = await client.ListLeasesAsync("auth/userpass/login/");
                Assert.NotNull(authUserpassLeases?.Data?.Keys);
                Assert.NotEmpty(authUserpassLeases.Data.Keys);
                Assert.Contains("foouser/", authUserpassLeases.Data.Keys);

                await client.DeleteUserpassUserAsync("foouser");
                authUserpassLeases = await client.ListLeasesAsync("auth/userpass/login/");
                // Can't test this because Vault does not clean up paths even after leases are revoked
                //Assert.DoesNotContain("foouser", authUserpassLeases.Data.Keys);

                await client.WriteGenericSecretAsync("some-random-secret", new { Foo = "Bar" });
                // Forces create of a wrapped response lease
                var secret = await client.ReadGenericSecretAsync("some-random-secret",
                        new GenericSecretOptions { WrapTtl = "5s" });

                var secretLeases = await client.ListLeasesAsync("secret/");
                Assert.Contains("some-random-secret/", secretLeases.Data.Keys);
                var secretRandomLeases = await client.ListLeasesAsync("secret/some-random-secret/");
                Assert.NotEmpty(secretRandomLeases.Data.Keys);
                // Wait for wrapped response lease to expire
                Thread.Sleep(5 * 1000);
                secretRandomLeases = await client.ListLeasesAsync("secret/some-random-secret/");
                Assert.Equal(0, (secretRandomLeases?.Data?.Keys?.Length).GetValueOrDefault());

                var badSecretLease = await client.ListLeasesAsync("secret/no-such-path/");
                Assert.Null(badSecretLease);

                await client.DeleteGenericSecretAsync("some-random-secret");
                secretLeases = await client.ListLeasesAsync("secret/");
                // Can't test this because Vault does not clean up paths even after leases are revoked
                //Assert.DoesNotContain("some-random-secret", secretLeases.Data.Keys);
            }
        }
    }
}