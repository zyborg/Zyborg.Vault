using System.Security;

namespace Zyborg.Vault.POSH.Model
{
	/// <summary>
	/// <para type="description">
	/// Captures the result of profile listing or retrieval from the
	/// user's profile store.
	/// </para>
	/// </summary>
	public class GetProfileResult
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public string VaultAddress
		{ get; set; }

		public SecureString VaultToken
		{ get; set; }

		public string Label
		{ get; set; }

		public GetProfileResult SetVaultToken(string t)
		{
			if (string.IsNullOrEmpty(t))
				VaultToken = null;
			else
			{
				var ss = new SecureString();
				foreach (var c in t)
					ss.AppendChar(c);
				VaultToken = ss;
			}

			return this;
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
