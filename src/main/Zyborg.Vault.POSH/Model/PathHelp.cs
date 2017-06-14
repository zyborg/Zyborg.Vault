using Newtonsoft.Json;

namespace Zyborg.Vault.POSH.Model
{
	/// <summary>
	/// <para type="description">Captures help content for a given Vault path.</para>
	/// </summary>
	public class PathHelp
	{
		/// <summary>
		/// <para type="description">Contains the help content.</para>
		/// </summary>
		[JsonProperty("help")]
		public string Help
		{ get; set; }

		/// <summary>
		/// <para type="description">Reference to additional related help material.</para>
		/// </summary>
		[JsonProperty("see_also")]
		public string SeeAlso
		{ get; set; }
	}
}
