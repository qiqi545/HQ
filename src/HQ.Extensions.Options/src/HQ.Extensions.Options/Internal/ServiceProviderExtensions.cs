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
using HQ.Common;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Options.Internal
{
	public static class ServiceProviderExtensions
	{
		public static bool TryBindOptions(this IServiceProvider serviceProvider, Type optionsWrapperType, bool validate,
			out object options)
		{
			// IOptions<T>
			var arguments = optionsWrapperType.GetGenericArguments();
			var resolved = arguments[0];
			while (resolved != null && resolved.IsGenericType)
			{
				resolved = resolved.IsGenericTypeDefinition
					? resolved.MakeGenericType(arguments) // IOptions<TService<T1,...TN>>
					: resolved.BaseType; // HubOptions<THub> -> HubOptions
			}

			var testingType = validate ? typeof(IValidOptions<>) : typeof(IOptions<>);

			var targetType = testingType.MakeGenericType(resolved);

			try
			{
				var instance = serviceProvider.GetService(targetType);
				var property = targetType.GetProperty(nameof(IOptions<object>.Value));
				options = property?.GetValue(instance);
				return options != null;
			}
			catch (Exception e)
			{
				options = new
				{
					Type = optionsWrapperType.GetInnerGenericTypeName(),
					ErrorType = e.GetType().Name,
					e.Message,
					e.StackTrace
				};
				return false;
			}
		}
	}
}