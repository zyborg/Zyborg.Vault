using Newtonsoft.Json;

namespace Zyborg.Vault.POSH
{
	public class VaultProfile
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Label
		{ get; set; }

		public string VaultAddress
		{ get; set; }

		public string VaultToken
		{ get; set; }
	}
}
