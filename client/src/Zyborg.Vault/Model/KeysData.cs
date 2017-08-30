using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class KeysData
    {
        [JsonProperty("keys")]
        public string[] Keys
        { get; set; }
    }
}