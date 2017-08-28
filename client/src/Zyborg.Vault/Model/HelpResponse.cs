using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class HelpResponse
    {
        [JsonProperty("help")]
        public string Help
        { get; set; }
    }
}