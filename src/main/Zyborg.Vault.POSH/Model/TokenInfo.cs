using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Zyborg.Vault.POSH.Model
{
	/// <summary>
	/// <para type="description">
	/// Captures details about authentication tokens.
	/// </para>
	/// </summary>
	public class TokenInfo
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		[JsonProperty("accessor")]
		public string Accessor
		{ get; set; }

		[JsonProperty("creation_time")]
		public int CreationTime
		{ get; set; }

		[JsonProperty("creation_ttl")]
		public int CreationTtl
		{ get; set; }

		[JsonProperty("display_name")]
		public string DisplayName
		{ get; set; }

		[JsonProperty("expire_time")]
		public string ExpireTime // TODO: fix this datatype
		{ get; set; }

		[JsonProperty("explicit_max_ttl")]
		public int ExplicitMaxTtl
		{ get; set; }

		[JsonProperty("id")]
		public string Id
		{ get; set; }

		[JsonProperty("issue_time")]
		public DateTime? IssueTime
		{ get; set; }

		[JsonProperty("meta")]
		public Dictionary<string, string> Meta
		{ get; set; }

		[JsonProperty("num_uses")]
		public int NumUses
		{ get; set; }

		[JsonProperty("orphan")]
		public bool Orphan
		{ get; set; }

		[JsonProperty("path")]
		public string Path
		{ get; set; }

		[JsonProperty("policies")]
		public string[] Policies
		{ get; set; }

		[JsonProperty("renewable")]
		public bool Renewable
		{ get; set; }

		[JsonProperty("ttl")]
		public int Ttl
		{ get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
