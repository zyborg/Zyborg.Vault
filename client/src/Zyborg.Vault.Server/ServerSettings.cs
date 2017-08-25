using System.Collections.Generic;

namespace Zyborg.Vault.Server
{
    public class ServerSettings
    {
        public string ClusterName
        { get; set; }

        public StorageSettings Storage
        { get; set; } = new StorageSettings();

        public ListenerSettings Listener
        { get; set; } = new ListenerSettings();


        public class StorageSettings
        {
            public string Type
            { get; set; } = "file";

            public Dictionary<string, string> Settings
            { get; set; }
        }

        public class ListenerSettings
        {
            public string Type
            { get; set; } = "tcp";

            public Dictionary<string, string> Settings
            { get; set; }
        }
    }
}