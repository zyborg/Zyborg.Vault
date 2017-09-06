using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zyborg.Vault.Model
{
    public class ReadRepeatedResponse<TData> : ReadResponse<TData>
    {
        [JsonExtensionData(WriteData = true, ReadData = false)]
        protected Dictionary<string, object> RepeatedData =>
                JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(Data));
    }
}