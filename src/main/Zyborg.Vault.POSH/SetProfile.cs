using System.Management.Automation;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Defines or updates a Vault connection profile.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// A connection profile is a named and persisted collection of
	/// Vault connection attributes, which may include the Vault endpoint
	/// URL address and a Vault authentication token.
	/// </para><para type="description">
	/// Most of the Vault commands (all the ones that interact directly with
	/// the Vault server) take an optional parameter <c>'-VaultProfile'</c>
	/// (or the alias <c>'-vp'</c>) to reference a named profile for the current
	/// user.  Additionally, any attributes defined in the profile may be
	/// individually overridden when being called upon.
	/// </para><para type="description">
	/// Beside explicitily defining a profile with this command, you can also
	/// save a profile as the result of a new authentication via the
	/// <c>'New-VltAuth'</c> command.
	/// </para>
	/// <para type="link">Get-VltProfile</para>
	/// <para type="link">New-VltAuth</para>
	/// </remarks>
	[Cmdlet(VerbsCommon.Set, "Profile",
			DefaultParameterSetName = DefaultParamSet)]
	public class SetProfile : PSCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string DefaultParamSet = VaultBaseCmdlet.DefaultParamSet;
		public const string RemoveParamSet = "Remove";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// Specifies the name under which to save the profile.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0)]
		public string SaveAs
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specify the Vault endpoint URL address to include in the profile.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 1,
				ParameterSetName = DefaultParamSet)]
		[Alias("va")]
		public string VaultAddress
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Specify the Vault authentication token to include in the profile.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false, Position = 2,
				ParameterSetName = DefaultParamSet)]
		[Alias("vt")]
		public string VaultToken
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Provides an optional human-friendly descriptive name to the profile
		/// for quick and easy recognition upon inspection.
		/// </para>	
		/// </summary>
		[Parameter(Mandatory = false, Position = 3,
				ParameterSetName = DefaultParamSet)]
		public string Label
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// Reference an existing profile whose attributes will be copied
		/// over unless overridden by other parameters.
		/// </para><para type="description">
		/// If no overrides are specified, this will create a copy of the
		/// existing profile being referenced.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false,
				ParameterSetName = DefaultParamSet)]
		[Alias("vp")]
		public string VaultProfile
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// This switch parameter must be specified whenever referencing
		/// a profile name that already exists.  Without it, en error will
		/// be raised.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = false,
				ParameterSetName = DefaultParamSet)]
		public SwitchParameter Force
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// This switch parameter is used to remove an existing profile.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true,
				ParameterSetName = RemoveParamSet)]
		public SwitchParameter Remove
		{ get; set; }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void EndProcessing()
		{
			VaultProfile vp = null;
			if (!string.IsNullOrEmpty(VaultProfile))
			{
				vp = Global.GetVaultProfile(this, VaultProfile);
			}

			var addr = VaultAddress ?? vp?.VaultAddress;
			var token = VaultToken ?? vp?.VaultToken;

			Global.SetVaultProfile(this, SaveAs, Remove.IsPresent, Force.IsPresent,
					addr, token, Label);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
