// Copyright (c) HQ.IO Corporation. All rights reserved.
// Usage is strictly forbidden if not under license.

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HQ.Common.AspNetCore
{
	public class DynamicControllerAttribute : Attribute, IControllerModelConvention
	{
		public void Apply(ControllerModel controller)
		{
			if (!controller.ControllerType.IsGenericType)
				return;
			var type = controller.ControllerType.GetGenericArguments();

			Contract.Assert(type.Length > 0 && type.Length < 3);

			switch (type.Length)
			{
				case 0:
					break;
				case 1:
				{
					var controllerName = controller.ControllerType.Name.Replace("Controller`1", string.Empty);
					controller.ControllerName = $"{controllerName}_{type[0].Name}";
					break;
				}
				case 2:
				{
					var controllerName = controller.ControllerType.Name.Replace("Controller`2", string.Empty);
					controller.ControllerName = $"{controllerName}_{type[0].Name}_{type[1].Name}";
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}


		}
	}
}