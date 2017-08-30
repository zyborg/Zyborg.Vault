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
        public const string VaultAddress = "http://local-fiddler-8200:8888";
        //public const string VaultAddress = "http://local-fiddler-5000:8888";

        // HC Vault
        private string _rootToken = "21bd1f5a-6eff-07fe-0184-a4358ae809c1";

        // // Mock Vault
        // private string _rootToken = "d1166ee5-f095-4f9f-843f-6dfc084b06c3";


        [Fact]
        public async void ConfirmUserpassAuthIsEnabled()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                var auths = await client.ListAuthBackendsAsync();
                var expectedMount = $"{UserpassAuthExtensions.DefaultMountName}/";
                Assert.Contains(expectedMount, auths.Data.Keys);
            }
        }

        [Fact]
        public async void ListUsersBeforeAdd()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                var users = await client.ListUserpassUsersAsync();
                Assert.Equal(0, (users?.Data?.Keys?.Count()).GetValueOrDefault());
            }
        }


        [Fact]
        public async void CreateUser()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                await client.CreateUserpassUserAsync("foo", "bar");

                var users = await client.ListUserpassUsersAsync();
                Assert.Contains("foo", users.Data.Keys);
            }
        }

        [Fact]
        public async void LoginUserWithBadPassword()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

                await client.DeleteUserpassUserAsync("foo");

                var users = await client.ListUserpassUsersAsync();
                Assert.False((users?.Data?.Keys?.Contains("foo")).GetValueOrDefault(),
                        "Deleted user does not exist");
            }
        }

        [Fact]
        public async void DeleteNonExistingUser()
        {
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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
            using (var client = new VaultClient(VaultAddress))
            {
                client.VaultToken = _rootToken;

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