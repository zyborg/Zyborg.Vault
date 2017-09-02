using System.Linq;
using System.Net;
using Xunit;
using Zyborg.Vault;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Ext.Userpass;
using Zyborg.Vault.Model;

namespace Zyborg.Vault
{
    public class UserpassAuthIntegrationTests
    {
        public const string TestVaultAddress = TestConfig.TestVaultAddress;
        public static readonly string TestRootToken = TestConfig.RootTokens[TestVaultAddress];


        [Fact]
        public async void ConfirmUserpassAuthIsEnabled()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var auths = await client.ListAuthBackendsAsync();
                var expectedMount = $"{UserpassAuthExtensions.DefaultMountName}/";
                Assert.Contains(expectedMount, auths.Data.Keys);
            }
        }

        [Fact]
        public async void ListUsersBeforeAdd()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var users = await client.ListUserpassUsersAsync();
                Assert.Equal(0, (users?.Data?.Keys?.Count()).GetValueOrDefault());
            }
        }


        [Fact]
        public async void CreateUser()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                await client.CreateUserpassUserAsync("foo", "bar");

                var users = await client.ListUserpassUsersAsync();
                Assert.Contains("foo", users.Data.Keys);
            }
        }

        [Fact]
        public async void LoginUserWithBadPassword()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var users = await client.ListUserpassUsersAsync();
                Assert.Contains("foo", users.Data.Keys);

                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.LoginUserpassAsync("foo", "with-bad-password"));
                
                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("invalid username or password", ex.Errors.Errors.First());
            }
        }

        [Fact]
        public async void LoginUser()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var users = await client.ListUserpassUsersAsync();
                Assert.Contains("foo", users.Data.Keys);

                var login = await client.LoginUserpassAsync("foo", "bar");
                
                Assert.Null(login.Data);
                Assert.NotNull(login?.Auth?.ClientToken);
                Assert.Contains("username", login.Auth.Metadata.Keys);
                Assert.Equal("foo", login.Auth.Metadata["username"]);
            }
        }

        [Fact]
        public async void DeleteUser()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                await client.DeleteUserpassUserAsync("foo");

                var users = await client.ListUserpassUsersAsync();
                Assert.False((users?.Data?.Keys?.Contains("foo")).GetValueOrDefault(),
                        "Deleted user does not exist");
            }
        }

        [Fact]
        public async void DeleteNonExistingUser()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var nonUser = "no-such-user-in-existence";
                var users = await client.ListUserpassUsersAsync();
                Assert.False((users?.Data?.Keys?.Contains(nonUser)).GetValueOrDefault(),
                        "Deleted user does not exist");
                await client.DeleteUserpassUserAsync(nonUser);
            }
        }

        [Fact]
        public async void LoginNonExistingUser()
        {
            using (var client = new VaultClient(TestVaultAddress))
            {
                client.VaultToken = TestRootToken;

                var nonUser = "no-such-user-in-existence";
                var users = await client.ListUserpassUsersAsync();
                Assert.False((users?.Data?.Keys?.Contains(nonUser)).GetValueOrDefault(),
                        "Deleted user does not exist");

                var ex = await Assert.ThrowsAsync<VaultClientException>(async () =>
                        await client.LoginUserpassAsync(nonUser, "no-such-password"));
                
                Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
                Assert.Equal("invalid username or password", ex.Errors.Errors.First());
            }
        }
    }
}