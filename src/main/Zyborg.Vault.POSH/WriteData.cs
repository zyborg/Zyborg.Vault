using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Write data (secrets or configuration) into Vault to a backend provider
	/// logically mounted at a specified path.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// There are 3 different Parameter Sets corresponding to 3 typical usages of
	/// this cmdlet.
	/// </para><para type="description">
	/// In the first (default) usage, a single vault path is specified
	/// and one or more key-value pairs are provided.  The Keys are all provided as
	/// an array, and the values are all provided as an array, and the length of
	/// these two arrays must be equal.  Additionally, the Keys and Values can be
	/// provided from the pipeline using objects with the corresponding property
	/// names.
	/// </para><para type="description">
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string DataMapParamSet = "DataMap";
		public const string NoValuesParamSet = "NoValues";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The path to which to write one or more key-value pairs of secrets.
		/// This parameter set allows you to write multiple key-values to a single path.
		/// </para><para type="description">
		/// The behavior of the write is determined by the secret provider mounted
		/// at the given path. For example, writing to "aws/policy/ops" will create
		/// an "ops" IAM policy for the AWS secret provider (configuration), but
		/// writing to "consul/foo" will write ta value directly into Consul at that
		/// key.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = DefaultParamSet)]
		public string Path
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specifies one or more keys to set a value for.
		/// </para><para type="description">
		/// The count of corresponding values specified with the <c>Value</c> 
		/// parameter must match the count of keys.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 1, ParameterSetName = DefaultParamSet,
				ValueFromPipelineByPropertyName = true)]
		[Alias("Name")]
		public string[] Key
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specifies one or more secret values to set for the corresponding keys.
		/// </para><para type="description">
		/// The count of corresponding keys specified with the <c>Key</c> parameter
		/// must match the count of values.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 2, ParameterSetName = DefaultParamSet,
				ValueFromPipelineByPropertyName = true)]
		public object[] Value
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// The paths to which to write one ore more key-value pairs.
		/// This parameter set allows you to write to multiple paths simultaneously.
		/// </para><para type="description">
		/// The behavior of the write is determined by the secret provider mounted
		/// at the given path. For example, writing to "aws/policy/ops" will create
		/// an "ops" IAM policy for the AWS secret provider (configuration), but
		/// writing to "consul/foo" will write ta value directly into Consul at that
		/// key.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = DataMapParamSet,
				ValueFromPipeline = true)]
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = NoValuesParamSet,
				ValueFromPipeline = true)]
		public string[] Paths
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// A map of key-value pairs to write to the target path or paths.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 1, ParameterSetName = DataMapParamSet)]
		public Hashtable Data
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Forces the write to without any data values specified.  This allows writing
		/// to keys that do not need or expect any fields to be specified.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 1, ParameterSetName = NoValuesParamSet)]
		public SwitchParameter NoValues
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// When specified, the returned result maintains the meta data wrapper
		/// for the secret result.
		/// </para>
		/// </summary>
		[Parameter()]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
