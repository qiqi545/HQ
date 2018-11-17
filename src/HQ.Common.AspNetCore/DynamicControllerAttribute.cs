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
