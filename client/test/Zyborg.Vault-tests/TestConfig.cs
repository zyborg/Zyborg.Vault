using System.Collections.Generic;
using System.Linq;

namespace Zyborg.Vault
{
    public class TestConfig
    {
        public const string TestVaultAddress = FileConfigVaultAddress;


        // Points to a Vault instance that has not yet been initialized (NO-INIT config)
        public const string NoInitConfigVaultAddress = "http://local-fiddler-8299:8888";

        // Points to a Vault instance that uses File-based Storage (FILE config)
        public const string FileConfigVaultAddress = "http://local-fiddler-8200:8888";
        
        // Points to a MockServer instance
        public const string MockServerVaultAddress = "http://local-fiddler-5000:8888";

        public static readonly IReadOnlyDictionary<string, IEnumerable<string>> UnsealKeys =
                new Dictionary<string, IEnumerable<string>>
                {
                    [FileConfigVaultAddress] =
                            new[]
                            {
                                "vault unseal efd737871f6fcd1464b71ce185d7c5cc6b89ef75e4e1b42cd01ce6996e06db16a1",
                                "vault unseal aa4efa7677eeae314e77b4c3044bd5b99cb2a231d7cbc9621f3052928e2af33195",
                                "vault unseal bbb8d156a1b80c96ad001540528115f3949dd32c9828e3072cfb63e662e1088efe",
                            }.Select(x => x.Replace("vault unseal ","")),
                    [MockServerVaultAddress] =
                            new[]
                            {
                                "vault unseal 6111CC357474407411E86AE9B5575603EAD6C12889B2692E35A07D322E75366A01",
                                "vault unseal 55F23CD6AD457CB688F1FBBCE68752E78D7F12A3FA360607E4459D31C9E64F1102",
                                "vault unseal B05A6C7E13A36801FF0F7D8FD73EA7BB5918AADA224A23E9ABEF34309497913803"
                            }.Select(x => x.Replace("vault unseal ","")),
                };

        public static readonly IReadOnlyDictionary<string, string> RootTokens =
                new Dictionary<string, string>
                {
                    [FileConfigVaultAddress] =
                            "21bd1f5a-6eff-07fe-0184-a4358ae809c1",
                    [MockServerVaultAddress] =
                            "d1166ee5-f095-4f9f-843f-6dfc084b06c3",
                };

    }
}