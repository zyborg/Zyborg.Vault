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
        public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";


        [Fact]
        public async void WriteSecretWithoutAuth()
        {
            using (var client = new VaultClient(VaultAddress))
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
            using (var client = new VaultClient(VaultAddress) {
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
            using (var client = new VaultClient(VaultAddress) {
                VaultToken = _rootToken,
            })
            {
                var help = await client.GetHelpAsync("secret/foo-bar");
            }
        }

        [Fact]
        public async void WriteSecret()
        {
            using (var client = new VaultClient(VaultAddress) {
                VaultToken = _rootToken,
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
