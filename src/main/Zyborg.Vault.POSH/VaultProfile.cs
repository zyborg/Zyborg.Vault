using Newtonsoft.Json;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="description">
	/// Defines the elements of a Vault connection profile.
	/// </para>
	/// </summary>
	public class VaultProfile
	{
		/// <summary>
		/// <para type="description">
		/// An optional human-friendly label for the profile.
		/// </para>
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Label
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// A URL for the Vault server endpoint.
		/// </para>
		/// </summary>
		public string VaultAddress
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// An authentication token to be used to identify the caller
		/// when interacting with the Vault server.
		/// </para>
		/// </summary>
		public string VaultToken
		{ get; set; }
	}
}
