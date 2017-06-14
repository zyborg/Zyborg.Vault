using Newtonsoft.Json;

namespace Zyborg.Vault.POSH.Model
{
	/// <summary>
	/// <para type="description">
	/// Captures the list of keys representing authentication token accessors.
	/// </para>
	/// </summary>
	public class TokenAccessorList
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		[JsonProperty("keys")]
		public string[] Keys
		{ get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
