using Newtonsoft.Json;
using System.Collections;
using VaultSharp.Backends.Audit.Models;

namespace Zyborg.Vault.POSH.Model
{
	/// <summary>
	/// <para type="description">
	/// Captures details of a mounted audit provider.
	/// </para>
	/// </summary>
	public class GenericAuditBackend : AuditBackend
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public GenericAuditBackend(string type)
		{
			BackendType = new AuditBackendType(type);
		}

		public override AuditBackendType BackendType
		{ get; }

		[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
		public IDictionary Options
		{ get; set; }

		[JsonProperty("local", NullValueHandling = NullValueHandling.Ignore)]
		public bool Local
		{ get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
