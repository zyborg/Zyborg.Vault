using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Zyborg.Security.Cryptography;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server
{
    public class MockServer
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);

        public ServerSettings Settings
        { get; } = new ServerSettings();

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
        }

        public void StartStorage()
        {
            if ("file" != Settings.Storage.Type)
                throw new NotSupportedException($"unsupported storage type: {Settings.Storage.Type}");

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
                State.UnsealKeys = null;
            }
            else
            {
                // TODO: try-catch this and confirm the error response
                byte[] keyBytes = Util.HexUtil.HexToByteArray(key);
                if (State.UnsealKeys == null)
                {
                    // TODO: research this
                    State.UnsealNonce = Guid.NewGuid().ToString();
                }

                if (State.UnsealKeys == null)
                {
                    State.UnsealKeys = new[] { keyBytes };
                }
                else
                {
                    var keys = State.UnsealKeys.Append(keyBytes).ToArray();
                    if (keys.Length < State.Durable.SecretThreshold)
                    {
                        State.UnsealKeys = keys;
                    }
                    else
                    {
                        // Either we succeed or we fail but
                        // we reset the unseal state regardless
                        State.UnsealNonce = null;
                        State.UnsealKeys = null;

                        // Combine the assembled keys
                        // to derive the true root key
                        Unseal(keys);

                        Health.Sealed = false;
                    }
                }
            }

            return GetSealStatus();
        }

        private void Unseal(byte[][] keys)
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
                Progress = (State.UnsealKeys?.Length).GetValueOrDefault(),
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