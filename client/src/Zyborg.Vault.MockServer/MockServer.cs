using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Zyborg.Security.Cryptography;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Model;
using Zyborg.Vault.MockServer.Auth;
using Zyborg.Vault.MockServer.Policy;
using Zyborg.Vault.MockServer.Secret;
using Zyborg.Vault.MockServer.Storage;

namespace Zyborg.Vault.MockServer
{
    public class MockServer
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);

        /// <summary>
        /// Byte length of the unseal keys.
        /// </summary>
        public const int UnsealKeyLength = 33;

        private Dictionary<string, AuthInfo> _tokens = new Dictionary<string, AuthInfo>();

        private Dictionary<string, PolicyDefinition> _policies = PolicyManager.GetBasePolicies();
        private PathMap<ISecretBackend> _reservedMounts = new PathMap<ISecretBackend>();
        private PathMap<ISecretBackend> _secretMounts = new PathMap<ISecretBackend>();
        private PathMap<IAuthBackend> _authMounts = new PathMap<IAuthBackend>();

        public MockServer(IServiceProvider di)
        {
            DI = di;
        }

        public IServiceProvider DI
        { get; }

        public ServerSettings Settings
        { get; } = new ServerSettings();

        public IStorage Storage
        { get; private set; }

        public HealthStatus Health
        { get; } = new HealthImpl();

        public ServerState State
        { get; } = new ServerState();

        public void Start()
        {
            // Assume we're not initialized yet
            Health.Initialized = false;
            Health.Sealed = true;
            Health.Standby = true;

            StartStorage().Wait();

            // Reserve the sys backend mount -- this will actually be intercepted
            // and handled by the Sys Controller
            _reservedMounts.Set("sys", new DummyBackend());
            // Reserve the cubbyhole mount -- TODO:  for now we just use a plain old
            // Generic secret but will eventually correct this
            _reservedMounts.Set("cubbyhole", new GenericSecretBackend(
                    new StorageWrapper(Storage, "sys-mounts/cubbyhole")));

            _authMounts.Set("userpass", new UserpassAuthBackend(
                    new StorageWrapper(Storage, "auth-mounts/userpass")));

            _secretMounts.Set("secret", new GenericSecretBackend(
                    new StorageWrapper(Storage, "secret-mounts/secret")));

            // _secretMounts.Set("alt-secret1", new GenericSecretBackend(
            //         new StorageWrapper(Storage, "secret-mounts/alt-secret1")));
            // _secretMounts.Set("alt/secret/second", new GenericSecretBackend(
            //         new StorageWrapper(Storage, "secret-mounts/alt/secret/second")));

            // Register root token
            var tokenId = State.Durable.RootTokenId;
            _tokens.Add(tokenId, new AuthInfo
            {
                Accessor = Guid.NewGuid().ToString(),
                ClientToken = tokenId,
                LeaseDuration = 0,
                Renewable = false,
                Policies = new[] { "root" },
            });
        }

        public async Task StartStorage()
        {
            if (!Standard.StorageTypes.TryGetValue(Settings.Storage.Type, out var storageType))
                throw new NotSupportedException($"unsupported storage type: {Settings.Storage.Type}");

            IStorage s = (IStorage)ActivatorUtilities.CreateInstance(DI, storageType);
            if (s == null)
                throw new NotSupportedException($"unresolved storage type: {Settings.Storage.Type}: {storageType}");
            this.Storage = s;

            var stateJson = await this.Storage.ReadAsync("server/state");
            if (stateJson != null)
            {
                State.Durable = JsonConvert.DeserializeObject<DurableServerState>(
                        stateJson);
                Health.Initialized = true;
            }
        }

        public async Task SaveState()
        {
            if (this.Storage == null)
                throw new InvalidOperationException("storage system has not been initialized");

            var ser = JsonConvert.SerializeObject(State.Durable, Formatting.Indented);
            await this.Storage.WriteAsync("server/state", ser);
        }

        public InitializationStatus GetInitializationStatus()
        {
            return new InitializationStatus
            {
                Initialized = Health.Initialized,
            };
        }

        public InitializationResponse Initialize(int n, int t)
        {
            if (Health.Initialized)
                return null;

            using (var aes = Aes.Create())
            using (var tss = ThresholdSecretSharingAlgorithm.Create())
            using (var sha = SHA512.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                var rootKeyClear = aes.Key;
                var rootKeyCrypt = tss.Split(rootKeyClear, n, t);
                var rootKeyShares = tss.Shares.ToArray();
                var rootToken = Guid.NewGuid();

                var resp = new InitializationResponse
                {
                    Keys = rootKeyShares.Select(x => BitConverter.ToString(x).Replace("-","")).ToArray(),
                    KeysBase64 = rootKeyShares.Select(x => Convert.ToBase64String(x)).ToArray(),
                    RootToken = rootToken.ToString(),
                };

                try
                {
                    State.Durable = new DurableServerState();
                    State.Durable.SecretShares = n;
                    State.Durable.SecretThreshold = t;
                    State.Durable.RootKeyTerm = 1;
                    State.Durable.RootKeyInstallTime = DateTime.UtcNow;
                    State.Durable.RootKeyEncrypted = rootKeyCrypt;
                    State.Durable.RootKeyHash = sha.ComputeHash(rootKeyClear);
                    State.Durable.RootTokenHash = sha.ComputeHash(rootToken.ToByteArray());

                    State.Durable.ClusterName = Settings.ClusterName;
                    State.Durable.ClusterId = Guid.NewGuid().ToString();

                    SaveState().Wait();
                    Health.Initialized = true;
                    return resp;
                }
                catch
                {
                    State.Durable = null;
                    throw;
                }
            }
        }

        public SealStatus Unseal(string key, bool reset)
        {
            if (reset)
            {
                State.UnsealNonce = null;
                State.UnsealProgress = null;
            }
            else
            {
                byte[] keyBytes;
                if (key.Length == UnsealKeyLength * 2)
                {
                    // Hex-encoded key

                    // TODO: try-catch this and confirm the error response
                    keyBytes = Vault.Util.HexUtil.HexToByteArray(key);
                }
                else if (key.Length == UnsealKeyLength * 4 / 3)
                {
                    // Base64-encoded key
                    keyBytes = Convert.FromBase64String(key);
                }
                else
                {
                    throw new ArgumentException("invalid length key", nameof(key));
                }

                if (State.UnsealProgress == null)
                {
                    // TODO: research this
                    State.UnsealNonce = Guid.NewGuid().ToString();
                    State.UnsealProgress = new List<string>(State.Durable.SecretThreshold);
                }

                var keyB64 = Convert.ToBase64String(keyBytes);
                if (!State.UnsealProgress.Contains(keyB64))
                    State.UnsealProgress.Add(keyB64);

                if (State.UnsealProgress.Count >= State.Durable.SecretThreshold)
                {
                    var keys = State.UnsealProgress.Select(x => Convert.FromBase64String(x)).ToArray();

                    // Either we succeed or we fail but
                    // we reset the unseal state regardless
                    State.UnsealNonce = null;
                    State.UnsealProgress.Clear();
                    State.UnsealProgress = null;

                    // Combine the assembled keys
                    // to derive the true root key
                    Unseal(keys);

                    // If we get here, we succeeded
                    State.UnsealKeys = keys;
                    Health.Sealed = false;
                }
            }

            return GetSealStatus();
        }

        private void Unseal(IEnumerable<byte[]> keys)
        {
            using (var tss = ThresholdSecretSharingAlgorithm.Create())
            using (var sha = SHA512.Create())
            {
                tss.Shares = keys;
                var rootKeyClear = tss.Combine(State.Durable.RootKeyEncrypted);
                var rootKeyHash = sha.ComputeHash(rootKeyClear);

                if (BitConverter.ToString(rootKeyHash) != BitConverter.ToString(State.Durable.RootKeyHash))
                    // TODO: verify the response in this case
                    throw new InvalidDataException("Invalid keys!");

                State.RootKey = rootKeyClear;
            }
        }

        public SealStatus GetSealStatus()
        {
            if (!Health.Initialized)
                return null;

            return new SealStatus
            {
                Sealed = Health.Sealed,
                SecretThreshold = State.Durable.SecretThreshold,
                SecretShares = State.Durable.SecretShares,
                Progress = (State.UnsealProgress?.Count).GetValueOrDefault(),
                Nonce = State.UnsealNonce ??string.Empty,
                Version = Health.Version,
                ClusterName = Health.ClusterName,
                ClusterId = Health.ClusterId,
            };
        }

        public KeyStatus GetKeyStatus()
        {
            if (!Health.Initialized || Health.Sealed)
                return null;

            return new KeyStatus
            {
                Term = State.Durable.RootKeyTerm.Value,
                InstallTime = State.Durable.RootKeyInstallTime.Value,
            };
        }

        public LeaderStatus GetLeaderStatus()
        {
            if (!Health.Initialized || Health.Sealed)
                return null;

            return new LeaderStatus
            {
                HaEnabled = false,
                IsSelf = true,
                LeaderAddress = "???",
            };
        }

        public IEnumerable<string> ListPolicies()
        {
            // TODO: move this to persistent operations
            return _policies.Keys.OrderBy(x => x);
        }

        public PolicyDefinition ReadPolicy(string name)
        {
            if (!_policies.TryGetValue(name, out var polDef))
                _policies.TryGetValue(name, out polDef);
            
            // TODO: move this to persistent operations
            return polDef;
        }

        public void WritePolicy(string name, string policyDefinition)
        {
            if (_policies.TryGetValue(name, out var polDef))
                if (polDef.IsUpdateForbidden)
                    throw new ArgumentException(
                            $"cannot update {name} policy");
            
            IPolicy pol;
            try
            {
                // Parse and validate the policy definition
                pol = PolicyManager.ParseDefinition(policyDefinition);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                        $"Failed to parse policy: {ex.GetBaseException().Message}");
            }

            if (polDef == null)
                polDef = new PolicyDefinition { Name = name };
            polDef.Definition = policyDefinition;
            polDef.Policy = pol;

            // TODO: move this to persistent operations
            _policies[name] = polDef;
        }

        public bool DeletePolicy(string name)
        {
            if (_policies.TryGetValue(name, out var polDef))
                if (polDef.IsDeleteForbidden)
                    throw new ArgumentException(
                            $"cannot delete {name} policy");

            // TODO: move this to persistent operations
            return _policies.Remove(name);
        }

        public void AssertAuthorized(string capability, string path,
                Dictionary<string, string> parameters = null, bool isSudo = false,
                params string[] policies)
        {
            var pols = PolicyManager.NoPolicies;
            if (policies != null && policies.Length > 0)
                pols = policies.Select(x => _policies.TryGetValue(x, out var pol)
                        ? pol.Policy : null).Where(x => x != null).ToArray();

            if (!PolicyManager.IsAuthorized(capability, path, parameters, isSudo, pols))
                throw new System.Security.SecurityException("permission denied");
        }

        public void AddToken(string id, AuthInfo auth)
        {
            _tokens[id] = auth;
        }

        public AuthInfo GetToken(string id)
        {
            _tokens.TryGetValue(id, out var t);
            return t;
        }

        public void DeleteToken(string id)
        {
            _tokens.Remove(id);
        }        

        public void GetAuthProviders()
        {

        }

        public void MountAuthProvider()
        {

        }

        public void DismountAuthProvider()
        {

        }

        public IEnumerable<string> ListAuthMounts()
        {
            return _authMounts.ListPaths();
        }

        public (IAuthBackend backend, string path) ResolveAuthMount(string mountAndPath)
        {
            if (string.IsNullOrEmpty(mountAndPath))
                return (null, null);

            string mount = mountAndPath;
            string path = string.Empty;

            while (!_authMounts.Exists(mount))
            {
                int lastSlash = mount.LastIndexOf('/');
                if (lastSlash <= 0)
                    // No more splitting and no match
                    return (null, null);
                
                path = $"{mount.Substring(lastSlash + 1)}/{path}";
                mount = mount.Substring(0, lastSlash);
            }

            return (_authMounts.Get(mount), path);
        }

        public IEnumerable<string> ListSecretMounts()
        {
            return _reservedMounts.ListPaths().Concat(_secretMounts.ListPaths());
        }

        public (ISecretBackend backend, string path) ResolveSecretMount(string mountAndPath)
        {
            if (string.IsNullOrEmpty(mountAndPath))
                return (null, null);

            string mount = mountAndPath;
            string path = string.Empty;

            while (!_secretMounts.Exists(mount))
            {
                int lastSlash = mount.LastIndexOf('/');
                if (lastSlash <= 0)
                    // No more splitting and no match
                    return (null, null);
                
                path = $"{mount.Substring(lastSlash + 1)}/{path}";
                mount = mount.Substring(0, lastSlash);
            }

            return (_secretMounts.Get(mount), path);
        }
    }

    public class HealthImpl : HealthStatus
    {

        public override string Version
        {
            get => typeof(MockServer).Assembly.GetName().Version.ToString();
            set => throw new NotSupportedException();
        }

        public override long ServerTimeUtc
        {
            get => (long)(DateTime.UtcNow - MockServer.UnixEpoch).TotalSeconds;
            set => throw new NotSupportedException();
        }
    }
}