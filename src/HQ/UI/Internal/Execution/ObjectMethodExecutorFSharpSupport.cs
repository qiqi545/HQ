#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.UI.Internal.Execution
{
	/// <summary>
	///     Helper for detecting whether a given type is FSharpAsync`1, and if so, supplying
	///     an <see cref="Expression" /> for mapping instances of that type to a C# awaitable.
	/// </summary>
	/// <remarks>
	///     The main design goal here is to avoid taking a compile-time dependency on
	///     FSharp.Core.dll, because non-F# applications wouldn't use it. So all the references
	///     to FSharp types have to be constructed dynamically at runtime.
	/// </remarks>
	internal static class ObjectMethodExecutorFSharpSupport
	{
		private static readonly object _fsharpValuesCacheLock = new object();
		private static Assembly _fsharpCoreAssembly;
		private static MethodInfo _fsharpAsyncStartAsTaskGenericMethod;
		private static PropertyInfo _fsharpOptionOfTaskCreationOptionsNoneProperty;
		private static PropertyInfo _fsharpOptionOfCancellationTokenNoneProperty;

		public static bool TryBuildCoercerFromFSharpAsyncToAwaitable(
			Type possibleFSharpAsyncType,
			out Expression coerceToAwaitableExpression,
			out Type awaitableType)
		{
			var methodReturnGenericType = possibleFSharpAsyncType.IsGenericType
				? possibleFSharpAsyncType.GetGenericTypeDefinition()
				: null;

			if (!IsFSharpAsyncOpenGenericType(methodReturnGenericType))
			{
				coerceToAwaitableExpression = null;
				awaitableType = null;
				return false;
			}

			var awaiterResultType = possibleFSharpAsyncType.GetGenericArguments().Single();
			awaitableType = typeof(Task<>).MakeGenericType(awaiterResultType);

			// coerceToAwaitableExpression = (object fsharpAsync) =>
			// {
			//     return (object)FSharpAsync.StartAsTask<TResult>(
			//         (Microsoft.FSharp.Control.FSharpAsync<TResult>)fsharpAsync,
			//         FSharpOption<TaskCreationOptions>.None,
			//         FSharpOption<CancellationToken>.None);
			// };
			var startAsTaskClosedMethod = _fsharpAsyncStartAsTaskGenericMethod
				.MakeGenericMethod(awaiterResultType);
			var coerceToAwaitableParam = Expression.Parameter(typeof(object));
			coerceToAwaitableExpression = Expression.Lambda(
				Expression.Convert(
					Expression.Call(
						startAsTaskClosedMethod,
						Expression.Convert(coerceToAwaitableParam, possibleFSharpAsyncType),
						Expression.MakeMemberAccess(null, _fsharpOptionOfTaskCreationOptionsNoneProperty),
						Expression.MakeMemberAccess(null, _fsharpOptionOfCancellationTokenNoneProperty)),
					typeof(object)),
				coerceToAwaitableParam);

			return true;
		}

		private static bool IsFSharpAsyncOpenGenericType(Type possibleFSharpAsyncGenericType)
		{
			var typeFullName = possibleFSharpAsyncGenericType?.FullName;
			if (!string.Equals(typeFullName, "Microsoft.FSharp.Control.FSharpAsync`1", StringComparison.Ordinal))
				return false;

			lock (_fsharpValuesCacheLock)
			{
				if (_fsharpCoreAssembly != null)
					return possibleFSharpAsyncGenericType.Assembly == _fsharpCoreAssembly;
				return TryPopulateFSharpValueCaches(possibleFSharpAsyncGenericType);
			}
		}

		private static bool TryPopulateFSharpValueCaches(Type possibleFSharpAsyncGenericType)
		{
			var assembly = possibleFSharpAsyncGenericType.Assembly;
			var fsharpOptionType = assembly.GetType("Microsoft.FSharp.Core.FSharpOption`1");
			var fsharpAsyncType = assembly.GetType("Microsoft.FSharp.Control.FSharpAsync");

			if (fsharpOptionType == null || fsharpAsyncType == null) return false;

			// Get a reference to FSharpOption<TaskCreationOptions>.None
			var fsharpOptionOfTaskCreationOptionsType = fsharpOptionType
				.MakeGenericType(typeof(TaskCreationOptions));
			_fsharpOptionOfTaskCreationOptionsNoneProperty = fsharpOptionOfTaskCreationOptionsType
				.GetTypeInfo()
				.GetRuntimeProperty("None");

			// Get a reference to FSharpOption<CancellationToken>.None
			var fsharpOptionOfCancellationTokenType = fsharpOptionType
				.MakeGenericType(typeof(CancellationToken));
			_fsharpOptionOfCancellationTokenNoneProperty = fsharpOptionOfCancellationTokenType
				.GetTypeInfo()
				.GetRuntimeProperty("None");

			// Get a reference to FSharpAsync.StartAsTask<>
			var fsharpAsyncMethods = fsharpAsyncType
				.GetRuntimeMethods()
				.Where(m => m.Name.Equals("StartAsTask", StringComparison.Ordinal));
			foreach (var candidateMethodInfo in fsharpAsyncMethods)
			{
				var parameters = candidateMethodInfo.GetParameters();
				if (parameters.Length == 3
				    && TypesHaveSameIdentity(parameters[0].ParameterType, possibleFSharpAsyncGenericType)
				    && parameters[1].ParameterType == fsharpOptionOfTaskCreationOptionsType
				    && parameters[2].ParameterType == fsharpOptionOfCancellationTokenType)
				{
					// This really does look like the correct method (and hence assembly).
					_fsharpAsyncStartAsTaskGenericMethod = candidateMethodInfo;
					_fsharpCoreAssembly = assembly;
					break;
				}
			}

			return _fsharpCoreAssembly != null;
		}

		private static bool TypesHaveSameIdentity(Type type1, Type type2)
		{
			return type1.Assembly == type2.Assembly
			       && string.Equals(type1.Namespace, type2.Namespace, StringComparison.Ordinal)
			       && string.Equals(type1.Name, type2.Name, StringComparison.Ordinal);
		}
	}
}