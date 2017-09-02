using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Ext.Token;
using Zyborg.Vault.Ext.Userpass;

namespace Zyborg.Vault
{
    public class SystemWrappingIntegrationTests
    {
        public const string TestVaultAddress = TestConfig.TestVaultAddress;
        public static readonly string TestRootToken = TestConfig.RootTokens[TestVaultAddress];


        [Fact]
        public async void WrapRewrapUnwrap()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var data1 = new TestClass
                {
                    SValue = "Test Value",
                    IValue = 42,
                    DTValue = DateTime.Now.AddSeconds(1234567),
                };

                // Create a wrapped response
                var wrapped1 = await client.WrapDataAsync(data1, "30s");
                Assert.Null(wrapped1.Data);
                Assert.NotNull(wrapped1.WrapInfo?.Token);

                // Verify it exists and we can look it up
                var lookup = await client.LookupWrappingAsync(wrapped1.WrapInfo.Token);
                Assert.Equal(wrapped1.WrapInfo.CreationPath, lookup.Data.CreationPath);
                Assert.Equal(wrapped1.WrapInfo.CreationTime, lookup.Data.CreationTime);
                Assert.Equal(wrapped1.WrapInfo.TTL, lookup.Data.CreationTtl);

                // Rewrap the wrapped response
                var wrapped2 = await client.RewrapDataAsync(wrapped1.WrapInfo.Token);
                Assert.NotNull(wrapped2.WrapInfo.Token);
                Assert.NotEqual(wrapped1.WrapInfo.Token, wrapped2.WrapInfo.Token);

                // Confirm the old wrapped token is gone
                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.LookupWrappingAsync(wrapped1.WrapInfo.Token));
                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("wrapping token is not valid or does not exist",
                        ex.Errors?.Errors?.FirstOrDefault());

                // Verify the ne re-wrapped token exists and we can look it up
                lookup = await client.LookupWrappingAsync(wrapped2.WrapInfo.Token);
                Assert.Equal(wrapped2.WrapInfo.CreationPath, lookup.Data.CreationPath);
                Assert.Equal(wrapped2.WrapInfo.CreationTime, lookup.Data.CreationTime);
                Assert.Equal(wrapped2.WrapInfo.TTL, lookup.Data.CreationTtl);

                // Resolve and read the wrapped response for the wrapping token...
                var val1 = await client.UnwrapData<TestClass>(wrapped2.WrapInfo.Token);
                // ...and confirm the value
                Assert.Equal(JsonConvert.SerializeObject(data1),
                        JsonConvert.SerializeObject(val1.Data));

                // Confirm the read wrapped token is gone
                ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.LookupWrappingAsync(wrapped2.WrapInfo.Token));
                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("wrapping token is not valid or does not exist",
                        ex.Errors?.Errors?.FirstOrDefault());
            }
        }

        public class TestClass
        {
            public string SValue
            { get; set; }

            public int IValue
            { get; set; }

            public DateTime DTValue
            { get; set; }

            public TestClass Child
            { get; set; }
        }
    }
}
