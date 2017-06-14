using System.Collections.Generic;
using System.Management.Automation;
using VaultSharp.Backends.System.Models;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Read data from Vault.
	/// </para><para type="synopsis">
	/// Comparable to the vault CLI <c>'read'</c> command.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// Reads data at the given path from Vault. This can be used to read secrets
	/// and configuration as well as generate dynamic values from materialized
	/// secret providers.  Please reference the documentation for the providers
	/// in use to determine key structure.
	/// </para><para type="description">
	/// </para>
	/// </remarks>
	[Cmdlet(VerbsCommunications.Read, "Data", DefaultParameterSetName = DefaultParamSet)]
	[OutputType(typeof(WrapInfo), ParameterSetName = new[] { WrapParamSet })]
	[OutputType(typeof(Dictionary<string, object>))]
	public class ReadData : VaultBaseCmdlet
	{
		/// <summary>
		/// <para type="description">
		/// One or more paths to read data from.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = DefaultParamSet)]
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = WrapParamSet)]
		public string[] Path
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Indicates that the response should be wrapped in a
		/// cubbyhole token with the requested TTL.
		/// </para><para type="description">
		/// This is a numeric string with an optional suffix "s", "m", or "h";
		/// if no suffix is specified it will be parsed as seconds.
		/// </para><para type="description">
		/// The unwrapped response can be fetched by calling the command
		/// again but providing the wrapped token ID to the <c>UnwrapToken</c>
		/// parameter.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = WrapParamSet)]
		public string WrapTtl
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// One ore more token ID representing wrapped data returned by a
		/// prior call of this command with the <c>WrappTtl</c> parameter
		/// set.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
				ParameterSetName = UnwrapParamSet)]
		public string[] UnwrapToken
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// When specified, the returned result maintains the meta data wrapper
		/// for the secret result.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false)]
		public SwitchParameter KeepSecretWrapper
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void BeginProcessing()
		{
			ResolveVaultClient();
		}

		protected override void ProcessRecord()
		{
			if (ParameterSetName == UnwrapParamSet)
			{
				foreach (var t in UnwrapToken)
				{
					var r = AsyncWaitFor(_client.UnwrapWrappedResponseDataAsync(t));
					WriteWrappedData(r, KeepSecretWrapper);
				}
			}
			else
			{
				foreach (var p in Path)
				{
					var r = AsyncWaitFor(_client.ReadSecretAsync(p));

					// Wrap
					if (!string.IsNullOrEmpty(WrapTtl))
					{
						var w = AsyncWaitFor(_client.WrapResponseDataAsync(r.Data, WrapTtl));
						WriteWrappedData(w, KeepSecretWrapper);
					}
					// Default
					else
					{
						WriteWrappedData(r, KeepSecretWrapper);
					}
				}
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
