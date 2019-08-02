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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HQ.UI.Internal.Execution
{
	internal struct AwaitableInfo
	{
		public Type AwaiterType { get; }
		public PropertyInfo AwaiterIsCompletedProperty { get; }
		public MethodInfo AwaiterGetResultMethod { get; }
		public MethodInfo AwaiterOnCompletedMethod { get; }
		public MethodInfo AwaiterUnsafeOnCompletedMethod { get; }
		public Type ResultType { get; }
		public MethodInfo GetAwaiterMethod { get; }

		public AwaitableInfo(
			Type awaiterType,
			PropertyInfo awaiterIsCompletedProperty,
			MethodInfo awaiterGetResultMethod,
			MethodInfo awaiterOnCompletedMethod,
			MethodInfo awaiterUnsafeOnCompletedMethod,
			Type resultType,
			MethodInfo getAwaiterMethod)
		{
			AwaiterType = awaiterType;
			AwaiterIsCompletedProperty = awaiterIsCompletedProperty;
			AwaiterGetResultMethod = awaiterGetResultMethod;
			AwaiterOnCompletedMethod = awaiterOnCompletedMethod;
			AwaiterUnsafeOnCompletedMethod = awaiterUnsafeOnCompletedMethod;
			ResultType = resultType;
			GetAwaiterMethod = getAwaiterMethod;
		}

		public static bool IsTypeAwaitable(Type type, out AwaitableInfo awaitableInfo)
		{
			// Based on Roslyn code: http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Shared/Extensions/ISymbolExtensions.cs,db4d48ba694b9347

			// Awaitable must have method matching "object GetAwaiter()"
			var getAwaiterMethod = type.GetRuntimeMethods().FirstOrDefault(m =>
				m.Name.Equals("GetAwaiter", StringComparison.OrdinalIgnoreCase)
				&& m.GetParameters().Length == 0
				&& m.ReturnType != null);
			if (getAwaiterMethod == null)
			{
				awaitableInfo = default;
				return false;
			}

			var awaiterType = getAwaiterMethod.ReturnType;

			// Awaiter must have property matching "bool IsCompleted { get; }"
			var isCompletedProperty = awaiterType.GetRuntimeProperties().FirstOrDefault(p =>
				p.Name.Equals("IsCompleted", StringComparison.OrdinalIgnoreCase)
				&& p.PropertyType == typeof(bool)
				&& p.GetMethod != null);
			if (isCompletedProperty == null)
			{
				awaitableInfo = default;
				return false;
			}

			// Awaiter must implement INotifyCompletion
			var awaiterInterfaces = awaiterType.GetInterfaces();
			var implementsINotifyCompletion = awaiterInterfaces.Any(t => t == typeof(INotifyCompletion));
			if (!implementsINotifyCompletion)
			{
				awaitableInfo = default;
				return false;
			}

			// INotifyCompletion supplies a method matching "void OnCompleted(Action action)"
			var iNotifyCompletionMap = awaiterType
				.GetTypeInfo()
				.GetRuntimeInterfaceMap(typeof(INotifyCompletion));
			var onCompletedMethod = iNotifyCompletionMap.InterfaceMethods.Single(m =>
				m.Name.Equals("OnCompleted", StringComparison.OrdinalIgnoreCase)
				&& m.ReturnType == typeof(void)
				&& m.GetParameters().Length == 1
				&& m.GetParameters()[0].ParameterType == typeof(Action));

			// Awaiter optionally implements ICriticalNotifyCompletion
			var implementsICriticalNotifyCompletion =
				awaiterInterfaces.Any(t => t == typeof(ICriticalNotifyCompletion));
			MethodInfo unsafeOnCompletedMethod;
			if (implementsICriticalNotifyCompletion)
			{
				// ICriticalNotifyCompletion supplies a method matching "void UnsafeOnCompleted(Action action)"
				var iCriticalNotifyCompletionMap = awaiterType
					.GetTypeInfo()
					.GetRuntimeInterfaceMap(typeof(ICriticalNotifyCompletion));
				unsafeOnCompletedMethod = iCriticalNotifyCompletionMap.InterfaceMethods.Single(m =>
					m.Name.Equals("UnsafeOnCompleted", StringComparison.OrdinalIgnoreCase)
					&& m.ReturnType == typeof(void)
					&& m.GetParameters().Length == 1
					&& m.GetParameters()[0].ParameterType == typeof(Action));
			}
			else
				unsafeOnCompletedMethod = null;

			// Awaiter must have method matching "void GetResult" or "T GetResult()"
			var getResultMethod = awaiterType.GetRuntimeMethods().FirstOrDefault(m =>
				m.Name.Equals("GetResult")
				&& m.GetParameters().Length == 0);
			if (getResultMethod == null)
			{
				awaitableInfo = default;
				return false;
			}

			awaitableInfo = new AwaitableInfo(
				awaiterType,
				isCompletedProperty,
				getResultMethod,
				onCompletedMethod,
				unsafeOnCompletedMethod,
				getResultMethod.ReturnType,
				getAwaiterMethod);
			return true;
		}
	}
}