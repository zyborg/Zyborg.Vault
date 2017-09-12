using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class UserpassAuthBackend : IAuthBackend
    {
        private Dictionary<string, string> _users = new Dictionary<string, string>();

        private static readonly JsonMergeSettings DefaultJsonMergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };

        private IStorage _storage;

        public UserpassAuthBackend(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<string>> ListAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);
            if ("users" == path)
            {
                var list = await _storage.ListAsync("users");

                // In order to preserve Vault compatibility we can't
                // simply return an empty list if there are no users
                // We need to trigger a 404 Not Found response
                if (list == null)
                    throw new NotSupportedException(string.Empty);
                return list;
            }
            else
                throw new NotSupportedException("unsupported path");
        }

        public async Task<string> ReadAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);
            Match m;
            if ((m = Regex.Match(path, "^users/([^/]+)$")).Success)
                return await ReadUser(m.Groups[1].Value);
            else
                throw new NotSupportedException("unsupported path");
        }

        public async Task WriteAsync(string path, string payload)
        {
            path = PathMap<object>.NormalizePath(path);
            Match m;
            if ((m = Regex.Match(path, "^users/([^/]+)$")).Success)
                await CreateOrUpdateUser(m.Groups[1].Value, payload);
            else if ((m = Regex.Match(path, "^users/([^/]+)/password$")).Success)
                await UpdateUserPassword(m.Groups[1].Value, payload);
            else
                throw new NotSupportedException("unsupported path");
        }

        public Task DeleteAsync(string path)
        {
            path = PathMap<object>.NormalizePath(path);
            throw new System.NotImplementedException();
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

        [SubWrite("users/{username}")]
        public async Task CreateOrUpdateUser(
                [Required, FromRoute]string username,
                [Required, FromBody]string payload)
        {
            var storagePath = $"users/{username}";
            var newUser = JObject.Parse(payload);
            var oldJson = await _storage.ReadAsync($"users/{username}");
            if (!string.IsNullOrEmpty(oldJson))
            {
                var oldUser = JObject.Parse(oldJson);
                oldUser.Merge(newUser, DefaultJsonMergeSettings);
                newUser = oldUser;
            }

            var user = newUser.ToObject<UserInfo>();
            if (string.IsNullOrEmpty(user.Password))
                throw new ArgumentException("missing password");

            await _storage.WriteAsync(storagePath, JsonConvert.SerializeObject(user));
        }

        /// <summary>
        /// Update password for an existing user.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="password">The password for the user.</param>
        [SubWrite("users/{username}/password")]
        public async Task UpdateUserPassword(
                [Required, FromRoute]string username,
                [Required, FromBody]string payload)
        {
            var storagePath = $"users/{username}";

            var userJson = await _storage.ReadAsync(storagePath);
            if (string.IsNullOrEmpty(userJson))
                throw new InvalidOperationException("username does not exist");

            var user = JsonConvert.DeserializeObject<UserInfo>(userJson);
            if (user == null)
                throw new InvalidOperationException("username does not exist");

            var p = JsonConvert.DeserializeObject<Dictionary<string, string>>(payload);
            if (!p.TryGetValue("password", out var password) || string.IsNullOrEmpty(password))
                throw new ArgumentException("missing password");

            user.Password = password;
            await _storage.WriteAsync(storagePath, JsonConvert.SerializeObject(user));
        }

        [SubRead("users/{username}")]
        public async Task<string> ReadUser([FromRoute]string username)
        {
            var storagePath = $"users/{username}";

            var userJson = await _storage.ReadAsync(storagePath);
            if (string.IsNullOrEmpty(userJson))
                throw new InvalidOperationException("username does not exist");

            var user = JsonConvert.DeserializeObject<UserInfo>(userJson);
            if (user == null)
                throw new InvalidOperationException("username does not exist");

            return await Task.FromResult(JsonConvert.SerializeObject(user));
        }

        public class UserInfo
        {
            /// <summary>
            /// The password for the user. Only required when creating the user.
            /// </summary>
            [Required]
            [JsonProperty("password")]
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
    }
}