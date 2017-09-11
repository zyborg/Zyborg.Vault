using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Zyborg.Security.Cryptography;
using Zyborg.Vault.Ext.SystemBackend;
using Zyborg.Vault.Model;
using Zyborg.Vault.Server.Auth;
using Zyborg.Vault.Server.Secret;
using Zyborg.Vault.Server.Storage;

namespace Zyborg.Vault.Server
{
    public class MockServer
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);

        /// <summary>
        /// Byte length of the unseal keys.
        /// </summary>
        public const int UnsealKeyLength = 33;

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

            StartStorage();

            // Reserve the sys backend mount -- this will actually be intercepted
            // and handled by the Sys Controller
            _reservedMounts.Set("sys", new DummyBackend());
            // Reserve the cubbyhole mount -- TODO:  for now we just use a plain old
            // Generic secret but will eventually correct this
            _reservedMounts.Set("cubbyhole", new GenericSecretBackend(
                    new StorageWrapper(Storage, "sys-mounts/cubbyhole")));

            _secretMounts.Set("secret", new GenericSecretBackend(
                    new StorageWrapper(Storage, "secret-mounts/secret")));
            // _secretMounts.Set("alt-secret1", new GenericSecretBackend(
            //         new StorageWrapper(Storage, "secret-mounts/alt-secret1")));
            // _secretMounts.Set("alt/secret/second", new GenericSecretBackend(
            //         new StorageWrapper(Storage, "secret-mounts/alt/secret/second")));
        }

        public void StartStorage()
        {
            if (!Standard.StorageTypes.TryGetValue(Settings.Storage.Type, out var storageType))
                throw new NotSupportedException($"unsupported storage type: {Settings.Storage.Type}");

            IStorage s = (IStorage)ActivatorUtilities.CreateInstance(DI, storageType);
            if (s == null)
                throw new NotSupportedException($"unresolved storage type: {Settings.Storage.Type}: {storageType}");
            this.Storage = s;

            if (!Settings.Storage.Settings.TryGetValue("path", out var path))
                path = "./data";
            
            State.StorageRootPath = Path.GetFullPath(path);
            State.StorageFilePath = Path.Combine(State.StorageRootPath, "_state");

            if (File.Exists(State.StorageFilePath))
            {
                State.Durable = JsonConvert.DeserializeObject<DurableServerState>(
                        File.ReadAllText(State.StorageFilePath));
                Health.Initialized = true;
            }
        }

        public void SaveState()
        {
            if (string.IsNullOrEmpty(State.StorageFilePath) || string.IsNullOrEmpty(State.StorageRootPath))
                throw new InvalidOperationException("storage system has not been initialized");
            
            if (!Directory.Exists(State.StorageRootPath))
                Directory.CreateDirectory(State.StorageRootPath);

            var ser = JsonConvert.SerializeObject(State.Durable, Formatting.Indented);
            File.WriteAllText(State.StorageFilePath, ser);
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

                    SaveState();
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

        public void GetAuthProviders()
        {

        }

        public void MountAuthProvider()
        {

        }

        public void DismountAuthProvider()
        {

        }

        public IEnumerable<string> ListSecretMounts()
        {
            return _reservedMounts.ListPaths().Concat(_secretMounts.ListPaths());
        }

        public (ISecretBackend backend, string path) ResolveSecretMount(string mountAndPath)
        {
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