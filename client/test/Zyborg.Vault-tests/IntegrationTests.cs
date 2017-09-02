using System;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;
using Zyborg.Testing.XunitExt;
using Zyborg.Vault.Model;

[assembly: TestCaseOrderer(
        DependencyTestCaseOrderer.Name,
        DependencyTestCaseOrderer.Assembly)]
[assembly: TestCollectionOrderer(
        DependencyTestCollectionOrderer.Name,
        DependencyTestCollectionOrderer.Assembly)]

namespace Zyborg.Vault
{
    public class IntegrationTests
    {
        public const string TestVaultAddress = TestConfig.TestVaultAddress;
        
        public static readonly string TestRootToken = TestConfig.RootTokens[TestVaultAddress];


        [Fact]
        public async void WriteSecretWithoutAuth()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.WriteAsync("secret/any-place-we-choose", new
                        {
                            a = 1,
                            B = 2,
                        }));
                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("missing client token", ex.Errors?.Errors?.FirstOrDefault());
            }
        }

        [Fact]
        public async void WriteSecretWithBadAuth()
        {
            using (var client = new VaultClient(TestVaultAddress) {
                VaultToken = "bad-token",
            })
            {
                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.WriteAsync("secret/any-place-we-choose", new
                        {
                            a = 1,
                            B = 2,
                        }));
                Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
                Assert.Equal("permission denied", ex.Errors?.Errors?.FirstOrDefault());
            }
        }


        [Fact]
        public async void GetHelpForReadSecret()
        {
            using (var client = new VaultClient(TestVaultAddress) {
                VaultToken = TestRootToken,
            })
            {
                var help = await client.GetHelpAsync("secret/foo-bar");
            }
        }

        [Fact]
        public async void WriteSecret()
        {
            using (var client = new VaultClient(TestVaultAddress) {
                VaultToken = TestRootToken,
            })
            {
                await client.WriteAsync("secret/foo2", new
                {
                    a = 1,
                    B = "Two",
                    c = DateTime.Now,
                    DDD = Encoding.UTF8.GetBytes("Hello World!!!"),
                });
            }
        }
    }
}
