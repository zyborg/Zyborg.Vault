using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.Token
{
    public class TokenParameterRequest
    {
        [JsonProperty("token")]
        public string Token
        { get; set; }
    }
}