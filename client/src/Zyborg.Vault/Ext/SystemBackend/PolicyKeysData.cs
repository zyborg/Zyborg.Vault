using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class PolicyKeysData : KeysData
    {
        [JsonProperty("policies")]
        public string[] Policies => base.Keys;
    }
}