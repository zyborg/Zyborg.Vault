using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;

namespace Zyborg.Vault
{
    public class SystemMountsIntegrationTests
    {
         public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";

        [Fact]
        public async void ListMounts()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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
    }
}