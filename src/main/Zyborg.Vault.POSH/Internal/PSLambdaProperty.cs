using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Zyborg.Vault.POSH.Internal
{
	/// <summary>
	/// A specialization of a <see cref="PSPropertyInfo"/> that supports defining
	/// getter and setter logic as lambdas.
	/// </summary>
	public class PSLambdaProperty<T> : PSPropertyInfo
	{
		private Func<T> _getter;
		private Action<T> _setter;

		/// <summary>
		/// Constructs with a name and a getter lambda, and an optional setter lambda.
		/// </summary>
		public PSLambdaProperty(string name, Func<T> getter, Action<T> setter = null)
		{
			SetMemberName(name);
			_getter = getter;
			_setter = setter;
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public override PSMemberTypes MemberType => PSMemberTypes.CodeProperty;

		public override string TypeNameOfValue => typeof(T).FullName;

		public override bool IsGettable => _getter != null;

		public override bool IsSettable => _setter != null;

		public override object Value
		{
			get => _getter();
			set => _setter((T)value);
		}

		public override PSMemberInfo Copy()
		{
			return new PSLambdaProperty<T>(Name, _getter, _setter);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	/// <summary>
	/// A specialization of a <see cref="PSPropertyInfo"/> that supports defining
	/// getter and setter logic as lambdas that take a context argument.
	/// </summary>
	public class PSLambdaProperty<T, TArg> : PSPropertyInfo
	{
		private TArg _arg;
		private Func<TArg, T> _getter;
		private Action<TArg, T> _setter;

		/// <summary>
		/// Constructs with a name and a getter lambda, and an optional setter lambda.
		/// </summary>
		public PSLambdaProperty(string name, TArg arg, Func<TArg, T> getter, Action<TArg, T> setter = null)
		{
			SetMemberName(name);
			_arg = arg;
			_getter = getter;
			_setter = setter;
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public override PSMemberTypes MemberType => PSMemberTypes.CodeProperty;

		public override string TypeNameOfValue => typeof(T).FullName;

		public override bool IsGettable => _getter != null;

		public override bool IsSettable => _setter != null;

		public override object Value
		{
			get => _getter(_arg);
			set => _setter(_arg, (T)value);
		}

		public override PSMemberInfo Copy()
		{
			return new PSLambdaProperty<T, TArg>(Name, _arg, _getter, _setter);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	/// <summary>
	/// A specialization of a <see cref="PSMethodInfo"/> that supports defining
	/// the invocation handling logic as a lambda.
	/// </summary>
	public class PSLambdaMethod : PSMethodInfo
	{
		private IEnumerable<Type> _argTypes;
		private Action<object[]> _handler;

		/// <summary>
		/// Constructs with a name and an invocation-handling lambda.
		/// </summary>
		public PSLambdaMethod(string name, IEnumerable<Type> argTypes, Action<object[]> handler)
		{
			SetMemberName(name);
			_argTypes = argTypes;
			_handler = handler;
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public override Collection<string> OverloadDefinitions
		{
			get
			{
				return new Collection<string>
				{
					PSLambdaMethod.ComputeDefinition(Name, null, _argTypes)
				};
			}
		}

		public override PSMemberTypes MemberType => PSMemberTypes.CodeMethod;

		// Consistent with PSCodeMethod implementation
		public override string TypeNameOfValue => typeof(PSCodeMethod).FullName;

		public override PSMemberInfo Copy()
		{
			return new PSLambdaMethod(Name, _argTypes, _handler);
		}

		public override object Invoke(params object[] arguments)
		{
			_handler(arguments);
			return null;
		}

		internal static string ComputeDefinition(string name, Type retType, IEnumerable<Type> argTypes)
		{
			var builder = new StringBuilder();
			if (retType != null)
				builder.Append(retType.FullName).Append(" ");
			builder.Append($"{name}(");
			if (argTypes != null)
				builder.Append(string.Join(", ", argTypes.Select(x => x.FullName)));
			builder.Append(")");
			return builder.ToString();
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	/// <summary>
	/// A specialization of a <see cref="PSMethodInfo"/> that supports defining
	/// the invocation handling logic as a lambda that returns a result.
	/// </summary>
	public class PSLambdaMethod<TResult> : PSMethodInfo
	{
		private IEnumerable<Type> _argTypes;
		private Func<object[], TResult> _handler;

		/// <summary>
		/// Constructs with a name and an invocation-handling lambda that returns a result.
		/// </summary>
		public PSLambdaMethod(string name, IEnumerable<Type> argTypes, Func<object[], TResult> handler)
		{
			SetMemberName(name);
			_argTypes = argTypes;
			_handler = handler;
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public override Collection<string> OverloadDefinitions
		{
			get
			{
				return new Collection<string>
				{
					PSLambdaMethod.ComputeDefinition(Name, typeof(TResult), _argTypes)
				};
			}
		}

		public override PSMemberTypes MemberType => PSMemberTypes.CodeMethod;

		// Consistent with PSCodeMethod implementation
		public override string TypeNameOfValue => typeof(PSCodeMethod).FullName;

		public override PSMemberInfo Copy()
		{
			return new PSLambdaMethod<TResult>(Name, _argTypes, _handler);
		}

		public override object Invoke(params object[] arguments)
		{
			return _handler(arguments);
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
