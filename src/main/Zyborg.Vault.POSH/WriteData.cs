using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// Write data (secrets or configuration) into Vault to a backend provider
	/// logically mounted at a specified path.
	/// </summary>
	/// <remarks>
	/// There are 3 different Parameter Sets corresponding to 3 typical usages of
	/// this cmdlet.
	/// <para>
	/// In the first (default) usage, a single vault path is specified
	/// and one or more key-value pairs are provided.  The Keys are all provided as
	/// an array, and the values are all provided as an array, and the length of
	/// these two arrays must be equal.  Additionally, the Keys and Values can be
	/// provided from the pipeline using objects with the corresponding property
	/// names.
	/// </para><para>
	/// In the second and third usages, either a single map of key-values is provided
	/// or no values are provided at all using the <c>-NoValues</c> switch (some
	/// vault backends support writing no value to trigger an action or behavior).
	/// Multiple vault paths are specified into which all will receive the same set
	/// of data values.  Tha Paths can be provided from the pipeline.
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommunications.Write, "Data", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(Secret<Dictionary<string, object>>))]
	public class WriteData : VaultBaseCmdlet
	{
		public const string DataMapParamSet = "DataMap";
		public const string NoValuesParamSet = "NoValues";

		// Single-Path, Multiple Key/Value sets
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = DefaultParamSet)]
		public string Path
		{ get; set; }

		[Parameter(Mandatory = true, Position = 1, ParameterSetName = DefaultParamSet,
				ValueFromPipelineByPropertyName = true)]
		[Alias("Name")]
		public string[] Key
		{ get; set; }

		[Parameter(Mandatory = true, Position = 2, ParameterSetName = DefaultParamSet,
				ValueFromPipelineByPropertyName = true)]
		public object[] Value
		{ get; set; }

		// Multiple Paths, Single or No Key/Value Map
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = DataMapParamSet,
				ValueFromPipeline = true)]
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = NoValuesParamSet,
				ValueFromPipeline = true)]
		public string[] Paths
		{ get; set; }

		[Parameter(Mandatory = true, Position = 1, ParameterSetName = DataMapParamSet)]
		public Hashtable Data
		{ get; set; }

		[Parameter(Mandatory = true, Position = 1, ParameterSetName = NoValuesParamSet)]
		public SwitchParameter NoValues
		{ get; set; }

		[Parameter()]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

		protected override void BeginProcessing()
		{
			base.ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			IDictionary<string, object> values = null;

			// This variation takes a single path, but multiple key-value pairs
			if (base.ParameterSetName == DefaultParamSet)
			{
				if (Key.Length != Value.Length)
					throw new ArgumentException("Key array parameter must match length of Value array parameter");
				values = new Dictionary<string, object>();
				for (int i = 0; i < Key.Length; ++i)
					values.Add(Key[i], Value[i]);

				var r = AsyncWaitFor(_client.WriteSecretAsync(Path, values));
				WriteWrappedData(r, KeepSecretWrapper);
				return;
			}

			// These variations take a single (or no) data map, but multiple Paths
			if (base.ParameterSetName == DataMapParamSet)
			{
				values = new Dictionary<string, object>();
				foreach (DictionaryEntry kv in Data)
					values.Add(kv.Key.ToString(), kv.Value);
			}
			else
			{
				// Assume NoValues
			}

			foreach (var p in Paths)
			{
				var r = AsyncWaitFor(_client.WriteSecretAsync(p, values));
				WriteWrappedData(r, KeepSecretWrapper);
			}
		}
	}
}
