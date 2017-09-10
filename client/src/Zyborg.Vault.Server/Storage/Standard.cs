using System;
using System.Collections.Generic;

namespace Zyborg.Vault.Server.Storage
{
    public static class Standard
    {
        public static readonly IReadOnlyDictionary<string, Type> StorageTypes =
                new Dictionary<string, Type>
                {
                    ["in-memory"] = typeof(InMemoryStorage),
                    ["file"] = typeof(FileStorage),
                    ["json-file"] = typeof(JsonFileStorage),
                };

        
    }
}