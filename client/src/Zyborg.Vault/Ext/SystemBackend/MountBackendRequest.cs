using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Vault.Ext.SystemBackend
{
    public class MountBackendRequest
    {
        /// <summary>
        /// Specifies the type of the backend, such as <c>aws</c>.
        /// </summary>
        [JsonProperty("type")]
        public string Type
        { get; set; }

        /// <summary>
        /// Specifies the human-friendly description of the mount.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description
        { get; set; }

        /// <summary>
        /// Specifies configuration options for this mount.
        /// </summary>
        /// <remarks>
        /// This is an object with these possible values:
        /// <list>
        ///   <item>default_lease_ttl</item>
        ///   <item>max_lease_ttl</item>
        ///   <item>force_no_cache</item>
        ///   <item>plugin_name</item>
        /// </list>
        /// These control the default and maximum lease time-to-live, and force disabling
        /// backend caching respectively. If set on a specific mount, this overrides the
        /// global defaults.
        /// </remarks>
        [JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Config
        { get; set; }

        /// <summary>
        /// Specifies if the secret backend is a local mount only.
        /// Local mounts are not replicated nor (if a secondary) removed by replication.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b><i>This option is allowed in Vault open-source, but relevant functionality
        /// is only supported in Vault Enterprise.</i></b>
        /// </para>
        /// </remarks>
        [JsonProperty("local", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Local
        { get; set; }
    }
}