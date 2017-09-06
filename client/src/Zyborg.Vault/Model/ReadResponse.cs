using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class ReadResponse<TData>
    {
        [JsonProperty("request_id")]
        public string RequestId
        { get; set; }

        [JsonProperty("lease_id")]
        public string LeaseId
        { get; set; }

        [JsonProperty("renewable")]
        public bool Renewable
        { get; set; }

        [JsonProperty("lease_duration")]
        public long LeaseDuration
        { get; set; }

        [JsonProperty("data")]
        public virtual TData Data
        { get; set; }

        [JsonProperty("wrap_info")]
        public WrapInfo WrapInfo
        { get; set; }

        [JsonProperty("warnings")]
        //public string[] Warnings
        public object Warnings
        { get; set; }

        [JsonProperty("auth")]
        public AuthInfo Auth
        { get; set; }
    }
}