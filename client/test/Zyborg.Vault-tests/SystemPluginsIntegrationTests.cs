using System.Linq;
using System.Net;
using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;

namespace Zyborg.Vault
{
    public class SystemPluginsIntegrationTests
    {
         public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";

        [Fact]
        public async void ListPlugins()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                var list = await client.ListPluginsAsync();
                // Some of the plugins we expect to be there in a default setup
                Assert.Superset(new[]
                        {
                            "mssql-database-plugin",
                            "mysql-database-plugin",
                            "postgresql-database-plugin",
                        }.ToHashSet(),
                        list.Data.Keys.ToHashSet());
            }
        }

        [Fact]
        public async void ReadNoSuchPlugin()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                // Query for a plugin we expect to be there in a default setup
                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.ReadPluginAsync("no-such-plugin"));
                
                Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
            }
        }

        [Fact]
        public async void ReadPlugin()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                // Query for a plugin we expect to be there in a default setup
                var info = await client.ReadPluginAsync("mssql-database-plugin");
                Assert.Equal("mssql-database-plugin", info.Data.Name);
            }
        }

        public const string MockPluginSha256 = "82DF2C9D63947E0C21AE97A76F7AF3549E40FA4B012DFD77BBC1B7BF971247FE";

        [Fact]
        public async void RegisterNoSuchPlugin()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                // Query for a plugin we expect to be there in a default setup
                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.RegisterPluginAsync("no-such-plugin",
                                MockPluginSha256, "no-such-plugin.exe"));

                Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
            }
        }

        [Fact(Skip = "Bad SHA is not detected at registration")]
        public async void RegisterBadShaPlugin()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                // Query for a plugin we expect to be there in a default setup
                //var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.RegisterPluginAsync("no-such-plugin",
                                new string(MockPluginSha256.ToCharArray().Reverse().ToArray()),
                                "mock-plugin.exe");
                                //);

                // No exception is thrown now -- only when the plugin is used
                //Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
 
                var info = await client.ReadPluginAsync("no-such-plugin");
                Assert.Equal("no-such-plugin", info.Data.Name);
           }
        }

        [Fact]
        public async void RegisterPlugin()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                await client.RegisterPluginAsync("mock-plugin",
                        new string(MockPluginSha256.ToCharArray().Reverse().ToArray()),
                        "mock-plugin.exe");

                var info = await client.ReadPluginAsync("mock-plugin");
                Assert.Equal("mock-plugin", info.Data.Name);

                await client.RemovePluginAsync("mock-plugin");

                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.ReadPluginAsync("mock-plugin"));
                
                Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
          }
        }
    }
}