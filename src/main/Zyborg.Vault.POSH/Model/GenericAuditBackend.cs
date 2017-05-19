using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp.Backends.Audit.Models;

namespace Zyborg.Vault.POSH.Model
{
	public class GenericAuditBackend : AuditBackend
	{
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
	}
}
