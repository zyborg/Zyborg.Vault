using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Vault.Model
{
    public abstract class Extensible
    {
        [JsonExtensionData]
        protected Dictionary<string, object> _extension = new Dictionary<string, object>();

        public virtual object this[string key] => _extension[key];
    }
}