using System;
using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public class KeyStatus
    {
        public int Term
        { get; set; }

        [JsonProperty("install_time")]
        public DateTime InstallTime
        { get; set; }
    }
}