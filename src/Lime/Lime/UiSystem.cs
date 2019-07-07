// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lime.Internal;
using Microsoft.Extensions.Primitives;
using TypeKitchen;

namespace Lime
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

		protected static void PopulateArguments(Ui ui, UiAction action, object target, MemberInfo callee, IDictionary<string, object> parameters)
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
							{
								arguments.Add(string.IsNullOrWhiteSpace(multiString) ? null : string.Join(",", multiString));
							}
							else
							{
								arguments.Add(multiString);
							}
						}
						else
						{
							arguments.Add(parameterValue);
						}
						continue;
					}

					// 
					// Inline context:
					var parameterType = parameter.ParameterType;
					if (parameterType == typeof(Ui))
					{
						ui = ui ?? InlineElements.GetUi() ?? throw new ArgumentNullException(nameof(ui),
								 $"No UI instance passed to PopulateAction or found in this synchronization context");

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
									{
										parameterValue = type.IsValueType
											? Instancing.CreateInstance(type)
											: null;
									}
								}
								else
								{
									parameterValue = ui.Context.UiServices.GetService(type);
								}
							}

							if (parameterValue is StringValues multiString)
							{
								parameterValue = type == typeof(string)
									? (object) string.Join(",", multiString)
									: multiString;
							}

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