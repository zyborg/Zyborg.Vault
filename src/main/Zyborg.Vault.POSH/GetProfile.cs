using System.Management.Automation;
using Zyborg.Vault.POSH.Model;

namespace Zyborg.Vault.POSH
{
	/// <summary>
	/// <para type="synopsis">
	/// Retrieves the list of Vault connection profiles defined for the
	/// current user/ or the details of a specific Vault connection profile.
	/// </para>
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "Profile",
			DefaultParameterSetName = DefaultParamSet)]
	public class GetProfile : PSCmdlet
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string DefaultParamSet = VaultBaseCmdlet.DefaultParamSet;
		public const string GetDetailsParamSet = "GetDetails";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// <para type="description">
		/// The name of one or more profiles for which to return the details.
		/// </para>
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ParameterSetName = GetDetailsParamSet,
				ValueFromPipeline = true)]
		public string[] VaultProfile
		{ get; set; }

		/// <summary>
		/// <para type="description">
		/// If this switch is specified, labels will be included in the result list.
		/// </para>
		/// </summary>
		[Parameter(ParameterSetName = DefaultParamSet)]
		public SwitchParameter ShowLabels
		{ get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		protected override void ProcessRecord()
		{
			if (VaultProfile == null)
			{
				foreach (var name in Global.GetVaultProfileNames(this))
				{
					if (ShowLabels)
					{
						var vp = Global.GetVaultProfile(this, name);
						WriteObject(new
						{
							Name = name,
							Label = vp.Label,
						});
					}
					else
					{
						WriteObject(name);
					}
				}
			}
			else
			{
				foreach (var name in VaultProfile)
				{
					var vp = Global.GetVaultProfile(this, name);
					if (vp != null)
					{
						WriteObject(new GetProfileResult
						{
							Label = vp.Label,
							VaultAddress = vp.VaultAddress,
						}.SetVaultToken(vp.VaultToken));
					}
				}
			}
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
