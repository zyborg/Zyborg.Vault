using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class RegisterPluginRequest
    {
        /// <summary>
        /// This is the SHA256 sum of the plugin's binary. Before a plugin
        /// is run it's SHA will be checked against this value, if they do
        /// not match the plugin can not be run.
        /// </summary>
        [JsonProperty("sha256")]
        public string Sha256
        { get; set; }

        /// <summary>
        /// Specifies the command used to execute the plugin. This is relative
        /// to the plugin directory. Example: <c>>myplugin --my_flag=1</c>
        /// </summary>
        [JsonProperty("command")]
        public string Command
        { get; set; }

        [JsonProperty("args", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Args
        { get; set; }
    }
}
