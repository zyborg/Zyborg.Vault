using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zyborg.Vault.Model;
using Zyborg.Vault.Server.Protocol;
using Zyborg.Vault.Server.Storage;

namespace Zyborg.Vault.Server.Auth
{
    /// <summary>
    /// For general information about the usage and operation of the
    /// Username and Password backend, please see the
    /// <see cref="https://www.vaultproject.io/docs/auth/userpass.html"
    /// >Vault Userpass backend documentation</see>.
    /// </summary>
    public class UserpassAuthBackend : LocallyRoutedAuthBackend<UserpassAuthBackend>, IAuthBackend
    {
        private static readonly JsonMergeSettings DefaultJsonMergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };

        private IStorage _storage;

        public UserpassAuthBackend(IStorage storage) : base(true)
        {
            _storage = storage;
        }

        /// <summary>
        /// List available userpass users.
        /// </summary>
        [LocalListRoute("users")]
        public async Task<IEnumerable<string>> ListUsers()
        {
            var list = await _storage.ListAsync("users");

            // In order to preserve Vault compatibility we can't
            // simply return an empty list if there are no users
            // We need to trigger a 404 Not Found response
            if (list == null || list.Count() == 0)
                throw new NotSupportedException(string.Empty);
            return list;
        }

        /// <summary>
        /// Reads the properties of an existing username.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        [LocalReadRoute("users/{username}")]
        public async Task<object> ReadUser([FromRoute]string username)
        {
            var storagePath = $"users/{username}";

            var userJson = await _storage.ReadAsync(storagePath);
            if (string.IsNullOrEmpty(userJson))
                throw new InvalidOperationException("username does not exist");

            var user = JsonConvert.DeserializeObject<UserInfo>(userJson);
            if (user == null)
                throw new InvalidOperationException("username does not exist");

            return await Task.FromResult(user.Config);
        }

        /// <summary>
        /// Create a new user or update an existing user.
        /// </summary>
        /// <remarks>
        /// This path honors the distinction between the create and update
        /// capabilities inside ACL policies.
        /// </remarks>
        /// <param name="username">
        /// The username for the user.
        /// </param>

        [LocalWriteRoute("users/{username}")]
        public async Task<object> CreateOrUpdateUser(
                [Required, FromRoute]string username,
                [Required, FromBody]string payload)
        {
            var storagePath = $"users/{username}";
            var newUser = JObject.Parse(payload);

            var userJson = await _storage.ReadAsync(storagePath);
            UserInfo user;
            if (!string.IsNullOrEmpty(userJson))
            {
                var jObj = JObject.Parse(userJson);
                var config = jObj[nameof(UserInfo.Config)] as JObject;
                if (config != null)
                    config.Merge(newUser);
                
                user = jObj.ToObject<UserInfo>();
            }
            else
            {
                user = new UserInfo
                {
                    Config = newUser.ToObject<UpdateUserRequest>()
                };
            }

            if (!string.IsNullOrEmpty(user.Config.Password))
            {
                user.PasswordHash = ComputePasswordHash(user.Config.Password);
                user.Config.Password = null;
            }

            if (user.PasswordHash == null)
                 throw new ArgumentException("missing password");

            await _storage.WriteAsync(storagePath, JsonConvert.SerializeObject(user));
            return null;
        }

        /// <summary>
        /// Update password for an existing user.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="password">The password for the user.</param>
        [LocalWriteRoute("users/{username}/password")]
        public async Task<object> UpdateUserPassword(
                [Required, FromRoute]string username,
                [Required, FromForm]string password)
        {
            var storagePath = $"users/{username}";

            var userJson = await _storage.ReadAsync(storagePath);
            if (string.IsNullOrEmpty(userJson))
                throw new InvalidOperationException("username does not exist");

            var user = JsonConvert.DeserializeObject<UserInfo>(userJson);
            if (user == null)
                throw new InvalidOperationException("username does not exist");

            user.PasswordHash = ComputePasswordHash(password);

            await _storage.WriteAsync(storagePath, JsonConvert.SerializeObject(user));
            return null;
        }

        /// <summary>
        /// Update policies for an existing user.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="policies">Comma-separated list of policies. If set to empty</param>
        [LocalWriteRoute("users/{username}/policies")]
        public async Task<object> UpdateUserPolicies(
                [Required, FromRoute]string username,
                [FromForm]string policies)
        {
            var storagePath = $"users/{username}";

            var userJson = await _storage.ReadAsync(storagePath);
            if (string.IsNullOrEmpty(userJson))
                throw new InvalidOperationException("username does not exist");

            var user = JsonConvert.DeserializeObject<UserInfo>(userJson);
            if (user == null)
                throw new InvalidOperationException("username does not exist");

            if (string.IsNullOrEmpty(policies))
                user.Config.Policies = "default";
            else
                user.Config.Policies = policies;
            
            await _storage.WriteAsync(storagePath, JsonConvert.SerializeObject(user));
            return null;
        }

        /// <summary>
        /// Deletes the user from the backend.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        [LocalDeleteRoute("users/{username}")]
        public async Task DeleteUserAsync(
                [Required, FromRoute]string username)
        {
            var storagePath = $"users/{username}";

            await _storage.DeleteAsync(storagePath);
        }

        /// <summary>
        /// Login with the username and password.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="password">The password for the user.</param>
        /// <returns></returns>
        [LocalWriteRoute("login/{username}")]
        public async Task<object> LoginUser(
                [Required, FromRoute]string username,
                // TODO:  HC Vault appears to throw a 500 Internal Server Error
                //        when this parameter is missing, but a 400 in other casese???
                [Required, FromForm]string password)
        {
            var storagePath = $"users/{username}";

            var userJson = await _storage.ReadAsync(storagePath);
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<UserInfo>(userJson);
                if (user != null && ComparePasswordAndHash(password, user.PasswordHash))
                    return new AuthInfo { ClientToken = Guid.NewGuid().ToString() };
            }

            throw new ArgumentException("invalid username or password");
        }

        internal static PasswordHash ComputePasswordHash(string passwordClear, byte[] salt = null)
        {
            // TODO: if this wasn't just a Mock Server
            //       and we really want to secure this,
            //       we would do hash using PBKDF+Scrypt
            return new PasswordHash { Hash = passwordClear };
        }

        internal static bool ComparePasswordAndHash(string passwordClear, PasswordHash passwordHash)
        {
            // TODO: if this wasn't just a Mock Server
            //       and we really want to secure this,
            //       we would do hash using PBKDF+Scrypt
            return string.Equals(passwordClear, passwordHash?.Hash);
        }

        public class UpdateUserRequest
        {
            /// <summary>
            /// The password for the user. Only required when creating the user.
            /// </summary>
            [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
            public string Password
            { get; set; }

            /// <summary>
            /// Comma-separated list of policies. If set to empty string, only the default policy will be applicable to the user.
            /// </summary>
            [JsonProperty("policies")]
            public string Policies
            { get; set; }

            /// <summary>
            /// The lease duration which decides login expiration.
            /// </summary>
            [JsonProperty("ttl")]
            public Duration? Ttl
            { get; set; }

            /// <summary>
            /// Maximum duration after which login should expire.
            /// </summary>
            [JsonProperty("max_ttl")]
            public Duration? MaxTtl
            { get; set; }
        }

        internal class UserInfo
        {
            public PasswordHash PasswordHash
            { get; set; }

            public UpdateUserRequest Config
            { get; set; }
        }

        internal class PasswordHash
        {
            public string Flag
            { get; } = "0";

            public string Hash
            { get; set; }

            public string Salt
            { get; set; }
        }
    }
}