using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Zyborg.Vault.POSH.Internal;

namespace Zyborg.Vault.POSH.Model
{
	/// <summary>
	/// <para type="description">
	/// This class represents a container for the credentials returned from initializing
	/// a Vault instance.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para type="description">
	/// This class is intentionally implemented in a non-straightforward way in order to
	/// capture and present all the contained elements in the best possible manner to the
	/// end user.
	/// </para><para type="description">
	/// If a user simply invokes the Init cmdlet without saving the returned output, the
	/// way this result object is defined will present to the user all of the elements
	/// such that they can be retroactively captured in a typical PS console.  (I.e. they
	/// will not accidentally lose the returned unseal keys and the root token.)
	/// </para><para type="description">
	/// Otherwise if the result is captured to a variable, then all the individual elements
	/// will be accessible as normal as dynamic properties.  Additionally the unseal keys
	/// can be accessed as a collection using a dynamically defined method.
	/// </para>
	/// </remarks>
	public class InitializeResult : PSObject
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public const string UnsealKeyCountProperty = "UnsealKeyCount";
		public const string UnsealKeyPropertyPrefix = "UnsealKey_";
		public const string RootTokenProperty = "RootToken";
		public const string GetUnsealKeysMethod = "GetUnsealKeys";

		private string[] _unsealKeys;
		public string _rootToken;

		internal InitializeResult(string[] unsealKeys, string rootToken)
		{
			_unsealKeys = unsealKeys.ToArray();
			_rootToken = rootToken;

			int i = 0;

			// Dynamically add a property to get the total count of Unseal keys
			base.Properties.Add(new PSLambdaProperty<int>(
					UnsealKeyCountProperty, () => _unsealKeys.Length));

			// Dynamically add each unseal key as a firt-order property which will
			// render as a top-level element under the default PS output formatting
			foreach (var uk in unsealKeys)
				base.Properties.Add(new PSLambdaProperty<string>(
						$"{UnsealKeyPropertyPrefix}{i++}", () => uk));

			// Dynamically add a method to get all unseal keys as a collection
			base.Methods.Add(new PSLambdaMethod<IEnumerable<string>>(
					GetUnsealKeysMethod, null, x => _unsealKeys));

			// Dynamically add a property to access the root token
			base.Properties.Add(new PSLambdaProperty<string>(
					RootTokenProperty, () => rootToken));
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
