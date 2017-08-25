using System;
using System.Linq;
using System.Net;
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
        //public const string VaultAddress = "http://local-fiddler-8200:8888";
        public const string VaultAddress = "http://local-fiddler-5000:8888";

        [Fact]
        public async void GetHealthBeforeInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var health = await client.GetHealthAsync();
                Assert.Equal(false, health.Initialized);
                Assert.Equal(true, health.Sealed);
                Assert.Equal(true, health.Standby);
            }
        }

        [Fact]
        public async void GetInitStatusBeforeInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var initStatus = await client.GetInitializationStatusAsync();
                Assert.Equal(false, initStatus.Initialized);
            }
        }

        [Fact]
        public async void GetSealStatusBeforeInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var ex = await Assert.ThrowsAsync<VaultClientException>(
                        async () => await client.GetSealStatusAsync());
                
                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("server is not yet initialized", ex.Errors?.Errors?.FirstOrDefault());
            }
        }

        [Fact]
        public async void GetKeyStatusBeforeInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var ex = await Assert.ThrowsAsync<VaultClientException>(
                        async () => await client.GetKeyStatusAsync());
                
                Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
                Assert.Equal("Vault is sealed", ex.Errors?.Errors?.FirstOrDefault());
            }
        }

        //[Fact(Skip = "Not Repeatable")]
        [Fact]
        public async void FirstInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var initRequ = new InitializationRequest
                {
                    SecretShares = 3,
                    SecretThreshold = 2,
                };

                var initResp = await client.DoInitializeAsync(initRequ);

                Assert.NotNull(initResp.RootToken);
                Assert.NotEqual(string.Empty, initResp.RootToken);

                Assert.NotNull(initResp.KeysBase64);
                Assert.NotEmpty(initResp.KeysBase64);
                Assert.Equal(initRequ.SecretShares, initResp.KeysBase64.Length);
            }
        }

        [Fact]
        public async void InitAfterInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var initRequ = new InitializationRequest
                {
                    SecretShares = 3,
                    SecretThreshold = 2,
                };

                var ex = await Assert.ThrowsAsync<VaultClientException>(
                        async () => await client.DoInitializeAsync(initRequ));

                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("Vault is already initialized", ex.Errors?.Errors?.FirstOrDefault());
            }
        }

        private string[] _keys = new[] {
            "efd737871f6fcd1464b71ce185d7c5cc6b89ef75e4e1b42cd01ce6996e06db16a1",
            "aa4efa7677eeae314e77b4c3044bd5b99cb2a231d7cbc9621f3052928e2af33195",
            "bbb8d156a1b80c96ad001540528115f3949dd32c9828e3072cfb63e662e1088efe",
        };

        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        [Fact]
        public async void PartialUnsealAfterInit()
        {

            using (var client = new VaultClient(VaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(_keys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);

                var partialKeys = _keys.Take(sealStatus.SecretThreshold - 1);
                foreach (var key in partialKeys)
                {
                    sealStatus = await client.DoUnsealAsync(new UnsealRequest { Key = key });
                    Assert.Equal(1, sealStatus.Progress);
                    Assert.Equal(true, sealStatus.Sealed);
                    Assert.Equal(_keys.Length, sealStatus.SecretShares);
                    Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold - 1);
                }

                sealStatus = await client.DoUnsealAsync(new UnsealRequest { Reset = true });
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(_keys.Length, sealStatus.SecretShares);
                Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold - 1);
            }
        }

        [Fact]
        public async void GetSealStatusBeforeUnseal()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(_keys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);
            }
        }

        [Fact]
        public async void UnsealAfterInit()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(_keys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);

                var partialKeys = _keys.Take(sealStatus.SecretThreshold);
                var expectedProgress = 1;
                foreach (var key in partialKeys)
                {
                    sealStatus = await client.DoUnsealAsync(new UnsealRequest { Key = key });
                    Assert.Equal(expectedProgress++, sealStatus.Progress);
                    Assert.Equal(true, sealStatus.Sealed);
                    Assert.Equal(_keys.Length, sealStatus.SecretShares);
                    Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold);

                    if (expectedProgress == partialKeys.Count())
                        // Break just before the threshold keys submitted
                        break;
                }

                // The last key submitted of threshold count behaves differently
                sealStatus = await client.DoUnsealAsync(new UnsealRequest { Key = partialKeys.Last() });
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(false, sealStatus.Sealed);
                Assert.Equal(_keys.Length, sealStatus.SecretShares);
                Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold);

                sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(false, sealStatus.Sealed);
            }
        }

        [Fact]
        public async void GetSealStatusAfterUnseal()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(false, sealStatus.Sealed);
                Assert.Equal(_keys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);
            }
        }

        [Fact]
        public async void SealAfterUnseal()
        {
            using (var client = new VaultClient(VaultAddress))
            {

                client.VaultToken = _rootToken;
                await client.DoSealAsync();

                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
            }
        }
    }
}
