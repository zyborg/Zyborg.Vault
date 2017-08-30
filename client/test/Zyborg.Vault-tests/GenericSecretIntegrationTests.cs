using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.GenericSecret;

namespace Zyborg.Vault
{
    public class GenericSecretIntegrationTests
    {
        public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";

        private static readonly Dictionary<string, object> SampleSecrets1 =
                new Dictionary<string, object>
                {
                    ["s1"] = "Value1",
                    ["s2"] = new DateTime(2009, 11, 16, 3, 35, 17),
                    ["s3"] = new[] { "one", "2", "III" },
                    ["s4"] = new Dictionary<string, object>
                    {
                        ["1"] = 1,
                        ["2"] = "two",
                        ["3"] = DateTime.Now,
                    },
                };

        private static readonly object SampleSecrets2 =
                new
                {
                    S1 = "Value1",
                    S2 = new DateTime(2009, 11, 16, 3, 35, 17),
                    S3 = new[] { "one", "2", "III" },
                    S4 = new
                    {
                        K1 = 1,
                        K2 = "two",
                        K3 = DateTime.Now,
                    },
                };

        [Fact]
        public async void ListNoSecrets()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                var list = await client.ListGenericSecretsAsync();
                Assert.Equal(0, (list?.Data?.Keys?.Where(x =>
                        new[] { "foo1","foo2" }.Contains(x))?.Count()).GetValueOrDefault());
            }
        }

        [Fact]
        public async void WriteSecretUsingDictionary()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                await client.WriteGenericSecretAsync("foo1", SampleSecrets1);
            }
        }

        [Fact]
        public async void WriteSecretUsingObject()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                await client.WriteGenericSecretAsync("foo2", SampleSecrets2);
            }
        }

        [Fact]
        public async void ReadSecretUsingDictionary()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                var secrets = await client.ReadGenericSecretAsync("foo1");
                var expected = SampleSecrets1.Keys.ToHashSet();
                var actual = secrets.Data.Keys.ToHashSet();

                Assert.Subset(expected, actual);
                Assert.Superset(expected, actual);

                //Assert.StrictEqual(SampleSecrets1, secrets.Data);
            }
        }

        [Fact]
        public async void ReadSecretUsingObject()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                var secrets = await client.ReadGenericSecretAsync("foo2");
                var expected = SampleSecrets2.GetType().GetProperties().Select(x => x.Name).ToHashSet();
                var actual = secrets.Data.Keys.ToHashSet();

                Assert.Subset(expected, actual);
                Assert.Superset(expected, actual);

                //Assert.StrictEqual(SampleSecrets2, secrets.Data);
            }
        }

        [Fact]
        public async void DeleteSecrets()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                await Task.WhenAll(
                    client.DeleteGenericSecretAsync("foo1"),
                    client.DeleteGenericSecretAsync("foo2"));
                
                var list = await client.ListGenericSecretsAsync();
                Assert.Equal(0, (list?.Data?.Keys?.Where(x =>
                        new[] { "foo1","foo2" }.Contains(x))?.Count()).GetValueOrDefault());
            }
        }
    }
}