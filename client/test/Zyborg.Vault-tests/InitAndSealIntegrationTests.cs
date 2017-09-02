using System;
using System.Linq;
using System.Net;
using Xunit;
using Zyborg.Testing.XunitExt;
using Zyborg.Vault.Model;
using Zyborg.Vault.Ext.SystemBackend;
using System.Collections.Generic;

namespace Zyborg.Vault
{
    public class InitAndSealIntegrationTests
    {
        public const string TestVaultAddress = TestConfig.TestVaultAddress;

        public readonly IEnumerable<string> TestUnsealKeys = TestConfig.UnsealKeys[TestVaultAddress];


        [Fact]
        public async void GetHealthBeforeInit()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                var health = await client.GetHealthAsync();
                Assert.Equal(false, health.Initialized);
                Assert.Equal(true, health.Sealed);
                Assert.Equal(true, health.Standby);
            }
        }

        [Fact]
        public async void GetLeaderStatusBeforeInit()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                var leaderStatus = await client.GetLeaderAsync();
            }
        }

        [Fact]
        public async void GetInitStatusBeforeInit()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                var initStatus = await client.GetInitializationStatusAsync();
                Assert.Equal(false, initStatus.Initialized);
            }
        }

        [Fact]
        public async void GetSealStatusBeforeInit()
        {
            using (var client = new VaultClient(TestVaultAddress))
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
            using (var client = new VaultClient(TestVaultAddress))
            {
                var ex = await Assert.ThrowsAsync<VaultClientException>(
                        async () => await client.GetKeyStatusAsync());

                Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
                Assert.Equal("Vault is sealed", ex.Errors?.Errors?.FirstOrDefault());
            }
        }

        [Fact(Skip = "Not Repeatable")]
        //[Fact]
        public async void FirstInit()
        {
            using (var client = new VaultClient(TestVaultAddress))
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
            using (var client = new VaultClient(TestVaultAddress))
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

        // // HC Vault
        // private string[] _keys = new[] {
        //     "efd737871f6fcd1464b71ce185d7c5cc6b89ef75e4e1b42cd01ce6996e06db16a1",
        //     "aa4efa7677eeae314e77b4c3044bd5b99cb2a231d7cbc9621f3052928e2af33195",
        //     "bbb8d156a1b80c96ad001540528115f3949dd32c9828e3072cfb63e662e1088efe",
        // };
        // private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // // Mock Vault
        // // private string[] _keys = new[] {
        // //     "6111CC357474407411E86AE9B5575603EAD6C12889B2692E35A07D322E75366A01",
        // //     "55F23CD6AD457CB688F1FBBCE68752E78D7F12A3FA360607E4459D31C9E64F1102",
        // //     "B05A6C7E13A36801FF0F7D8FD73EA7BB5918AADA224A23E9ABEF34309497913803"
        // // };
        // // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";

        [Fact]
        public async void ResetUnsealAfterInit()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                await client.DoUnsealAsync(new UnsealRequest { Reset = true });
            }
        }


        [Fact]
        public async void PartialUnsealAfterInit()
        {
            var unsealKeys = TestUnsealKeys.ToArray();

            using (var client = new VaultClient(TestVaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                //Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);

                var partialKeys = unsealKeys.Take(sealStatus.SecretThreshold - 1);
                foreach (var key in partialKeys)
                {
                    sealStatus = await client.DoUnsealAsync(new UnsealRequest { Key = key });
                    Assert.Equal(1, sealStatus.Progress);
                    Assert.Equal(true, sealStatus.Sealed);
                    Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                    Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold - 1);
                }

                sealStatus = await client.DoUnsealAsync(new UnsealRequest { Reset = true });
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold - 1);
            }
        }

        [Fact]
        public async void GetSealStatusBeforeUnseal()
        {
            var unsealKeys = TestUnsealKeys.ToArray();

            using (var client = new VaultClient(TestVaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);
            }
        }

        [Fact]
        public async void UnsealAfterInit()
        {
            var unsealKeys = TestUnsealKeys.ToArray();

            using (var client = new VaultClient(TestVaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
                Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);

                var partialKeys = unsealKeys.Take(sealStatus.SecretThreshold);
                var expectedProgress = 1;
                foreach (var key in partialKeys)
                {
                    sealStatus = await client.DoUnsealAsync(new UnsealRequest { Key = key });
                    Assert.Equal(expectedProgress++, sealStatus.Progress);
                    Assert.Equal(true, sealStatus.Sealed);
                    Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                    Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold);

                    if (expectedProgress == partialKeys.Count())
                        // Break just before the threshold keys submitted
                        break;
                }

                // The last key submitted of threshold count behaves differently
                sealStatus = await client.DoUnsealAsync(new UnsealRequest { Key = partialKeys.Last() });
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(false, sealStatus.Sealed);
                Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                Assert.Equal(partialKeys.Count(), sealStatus.SecretThreshold);

                sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(false, sealStatus.Sealed);
            }
        }

        [Fact]
        public async void GetSealStatusAfterUnseal()
        {
            var unsealKeys = TestUnsealKeys.ToArray();

            using (var client = new VaultClient(TestVaultAddress))
            {
                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(false, sealStatus.Sealed);
                Assert.Equal(unsealKeys.Length, sealStatus.SecretShares);
                Assert.True(sealStatus.SecretShares > sealStatus.SecretThreshold);
            }
        }

        [Fact]
        public async void SealAfterUnseal()
        {
            var rootToken = TestConfig.RootTokens[TestConfig.TestVaultAddress];

            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = rootToken;
                await client.DoSealAsync();

                var sealStatus = await client.GetSealStatusAsync();
                Assert.Equal(0, sealStatus.Progress);
                Assert.Equal(true, sealStatus.Sealed);
            }
        }
    }
}
