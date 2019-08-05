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
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TypeKitchen;

namespace HQ.Common.AspNetCore.Mvc
{
	public sealed class DynamicControllerAttribute : Attribute, IControllerModelConvention
	{
		public void Apply(ControllerModel controller)
		{
			if (!controller.ControllerType.IsGenericType)
				return;

			var types = controller.ControllerType.GetGenericArguments();
			if (types.Length == 0)
				return;

			controller.ControllerName = Pooling.StringBuilderPool.Scoped(sb =>
			{
				var controllerName = controller.ControllerType.Name.Replace($"Controller`{types.Length}", string.Empty);
				sb.Append(controllerName);
				foreach (var type in types)
					sb.Append($"_{type.Name}");
			});
		}
	}
}