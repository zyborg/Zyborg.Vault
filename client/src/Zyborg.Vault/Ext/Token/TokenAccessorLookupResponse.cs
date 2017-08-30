using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.Token
{
    public class TokenAccessorLookupResponse
    {
        [JsonProperty("accessor")]
        public string Accessor
        { get; set; }
    }
}