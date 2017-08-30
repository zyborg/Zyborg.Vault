using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    /// <summary>
    /// A JSON-encoded object including the (possibly encrypted, if pgp_keys was provided)
    /// master keys, base 64 encoded master keys and initial root token
    /// </summary>
    public class InitializationResponse
    {
        public string[] Keys
        { get; set; }

        [JsonProperty("keys_base64")]
        public string[] KeysBase64
        { get; set; }

        [JsonProperty("root_token")]
        public string RootToken
        { get; set; }
    }
}