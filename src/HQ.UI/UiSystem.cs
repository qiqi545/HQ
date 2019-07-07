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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HQ.UI.Internal;
using Microsoft.Extensions.Primitives;
using TypeKitchen;

namespace HQ.UI
{
	public abstract class UiSystem
	{
		public abstract void Begin(UiContext context = null);
		public abstract void End();

		public virtual void Error(string errorMessage, Exception exception = null)
		{
			Trace.WriteLine($"UI error: {errorMessage} {(exception == null ? "" : $"{exception}")}");
		}

		public virtual void PopulateAction(Ui ui, UiSettings settings, UiAction action, string template, object target,
			MethodInfo callee = null)
		{
			action.MethodName = callee?.Name ?? template ?? settings.DefaultPageMethodName;
			action.Arguments = null;
		}

		protected static void PopulateArguments(Ui ui, UiAction action, object target, MemberInfo callee,
			IDictionary<string, object> parameters)
		{
			var targetType = callee?.DeclaringType ?? target.GetType();
			var parameterValues = parameters.Values.ToArray();
			var executor = targetType.GetExecutor(action.MethodName, parameterValues);
			if (executor.SameMethodParameters(parameterValues))
			{
				action.Arguments = parameterValues;
				return;
			}

			var arguments = Pooling.ListPool<object>.Get();
			try
			{
				foreach (var parameter in executor.MethodParameters)
				{
					//
					// Exact match:
					if (parameters.TryGetValue(parameter.Name, out var parameterValue))
					{
						if (parameterValue is StringValues multiString)
						{
							if (parameter.ParameterType == typeof(string))
								arguments.Add(string.IsNullOrWhiteSpace(multiString)
									? null
									: string.Join(",", multiString));
							else
								arguments.Add(multiString);
						}
						else
							arguments.Add(parameterValue);

						continue;
					}

					// 
					// Inline context:
					var parameterType = parameter.ParameterType;
					if (parameterType == typeof(Ui))
					{
						ui = ui ?? InlineElements.GetUi() ?? throw new ArgumentNullException(nameof(ui),
							     "No UI instance passed to PopulateAction or found in this synchronization context");

						arguments.Add(ui);
						continue;
					}

					//
					// Model binding:
					if (parameterType.IsClass && !parameterType.IsAbstract)
					{
						var accessor = WriteAccessor.Create(parameterType, out var members);
						var instance = Instancing.CreateInstance(parameterType);

						foreach (var member in members)
						{
							var type = member.Type;

							if (!parameters.TryGetValue(member.Name, out parameterValue))
							{
								if (NotResolvableByContainer(type))
								{
									if (!type.IsNullableEnum() && type != null)
										parameterValue = type.IsValueType
											? Instancing.CreateInstance(type)
											: null;
								}
								else
									parameterValue = ui.Context.UiServices.GetService(type);
							}

							if (parameterValue is StringValues multiString)
								parameterValue = type == typeof(string)
									? (object) string.Join(",", multiString)
									: multiString;

							accessor.TrySetValue(instance, member.Name, parameterValue);
						}

						arguments.Add(instance);
						continue;
					}

					// 
					// Fallback instancing:
					if (NotResolvableByContainer(parameterType))
					{
						arguments.Add(parameterType.IsValueType
							? Instancing.CreateInstance(parameterType)
							: null);
						continue;
					}

					//
					// Dependency resolution:
					var argument = ui.Context.UiServices.GetService(parameterType);
					arguments.Add(argument);
				}

				action.Arguments = arguments.ToArray();
			}
			finally
			{
				Pooling.ListPool<object>.Return(arguments);
			}
		}

		private static bool NotResolvableByContainer(Type type)
		{
			return type.IsValueTypeOrNullableValueType();
		}
	}
}