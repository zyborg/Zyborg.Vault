using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Model;

namespace Zyborg.Vault
{
    public class SystemMountsIntegrationTests
    {
        public const string TestVaultAddress = TestConfig.TestVaultAddress;

        public static readonly string TestRootToken = TestConfig.RootTokens[TestVaultAddress];


        [Fact]
        public async void ListMounts()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var list = await client.ListMountedBackendsAsync();

                // Some of the mounts we expect to be there in a default setup
                Assert.Superset(new[]
                        {
                            "sys/",
                            "cubbyhole/",
                            "secret/",
                        }.ToHashSet(),
                        list.Data.Keys.ToHashSet());
            }
        }
        
        [Fact]
        public async void ReadMountConfiguration()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                // Some of the config elements we expect to be there in a default setup
                var expectedConfigAndTypes = new Dictionary<string, Type>
                {
                    ["default_lease_ttl"] = typeof(int),
                    ["max_lease_ttl"] = typeof(int),
                    ["force_no_cache"] = typeof(bool),
                };

                var config = await client.ReadMountConfigurationAsync("sys/");

                Assert.Superset(
                        expectedConfigAndTypes.Keys.ToHashSet(),
                        config.Data.Keys.ToHashSet());
            }
        }
        
        [Fact]
        public async void MountRemountUnmountBackend()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var list = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("test-generic/", list.Data.Keys);

                await client.MountBackendAsync("test-generic", "generic", "TEST GENERIC MOUNT");

                list = await client.ListMountedBackendsAsync();
                Assert.Contains("test-generic/", list.Data.Keys);
                Assert.Equal("generic", list.Data["test-generic/"].Type);

                await client.RemountBackendAsync("test-generic", "new-test-generic");

                list = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("test-generic/", list.Data.Keys);
                Assert.Contains("new-test-generic/", list.Data.Keys);
                Assert.Equal("generic", list.Data["new-test-generic/"].Type);

                await client.UnmountBackendAsync("new-test-generic");

                list = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("test-generic/", list.Data.Keys);
                Assert.DoesNotContain("new-test-generic/", list.Data.Keys);
            }
        }

        [Fact]
        public async void MountOverlapPrevented()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var mounts = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("a/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/d/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/d/e/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/d/e/f/", mounts.Data.Keys);

                try
                {
                    await client.MountBackendAsync("a/b/c", "generic");
                    mounts = await client.ListMountedBackendsAsync();
                    Assert.Contains("a/b/c/", mounts.Data.Keys);


                    var ex = await Assert.ThrowsAsync<VaultClientException>(
                            async () => await client.MountBackendAsync("a/b/c/d", "generic"));
                    Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                    Assert.Equal("existing mount at a/b/c/", ex.Errors.Errors.First());

                    mounts = await client.ListMountedBackendsAsync();
                    Assert.DoesNotContain("a/b/c/d/", mounts.Data.Keys);
                }
                finally
                {
                    await client.UnmountBackendAsync("a/b/c/d");
                    await client.UnmountBackendAsync("a/b/c");
                }

                mounts = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("a/b/c/d/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/", mounts.Data.Keys);
            }
        }

        [Fact]
        public async void LongestMountWins()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var mounts = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("a/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/d/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/d/e/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/c/d/e/f/", mounts.Data.Keys);

                try
                {
                    await client.MountBackendAsync("a/b/c/d/", "generic");
                    mounts = await client.ListMountedBackendsAsync();
                    Assert.Contains("a/b/c/d/", mounts.Data.Keys);

                    await client.WriteAsync("a/b/c/d/e/f", new TestClass1 { Key1 = "val1" });
                    var read = await client.ReadAsync<ReadResponse<TestClass1>>("a/b/c/d/e/f");
                    Assert.Equal("val1", read.Data.Key1);

                    await client.MountBackendAsync("a/b", "generic");
                    mounts = await client.ListMountedBackendsAsync();
                    Assert.Contains("a/b/", mounts.Data.Keys);

                    await client.WriteAsync("a/b/c/d/e/f", new TestClass1 { Key1 = "val1" });
                    read = await client.ReadAsync<ReadResponse<TestClass1>>("a/b/c/d/e/f");
                    Assert.Equal("val1", read.Data.Key1);

                    await client.UnmountBackendAsync("a/b/c/d");
                    mounts = await client.ListMountedBackendsAsync();
                    Assert.DoesNotContain("a/b/c/d/", mounts.Data.Keys);

                    var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                            read = await client.ReadAsync<ReadResponse<TestClass1>>("a/b/c/d/e/f"));
                    Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);

                    await client.WriteAsync("a/b/c/d/e/f", new TestClass1 { Key1 = "VAL2" });
                    read = await client.ReadAsync<ReadResponse<TestClass1>>("a/b/c/d/e/f");
                    Assert.Equal("VAL2", read.Data.Key1);
               }
                finally
                {
                    await client.UnmountBackendAsync("a/b/c/d");
                    await client.UnmountBackendAsync("a/b");
                }

                mounts = await client.ListMountedBackendsAsync();
                Assert.DoesNotContain("a/b/c/d/", mounts.Data.Keys);
                Assert.DoesNotContain("a/b/", mounts.Data.Keys);
            }
        }

        partial class TestClass1
        {
            public string Key1
            { get; set; }

            public int Key2
            { get; set; }

            public DateTime? Key3
            { get; set; }
        }
    }
}