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

using System.Linq;
using HQ.Common;
using HQ.Data.Contracts.Attributes;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace HQ.Platform.Api.Models
{
    public class VersionActionMethodSelectorAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (!routeContext.HttpContext.Items.TryGetValue(Constants.ContextKeys.Version, out var value))
                return true; // no version context, so we can't rule anything out

            if (!(value is VersionContext versionContext))
                return true; // developer error?

            if (!(action is ControllerActionDescriptor controllerActionDescriptor))
                return true; // unexpected level of abstraction

            if (!versionContext.Map.ContainsKey(controllerActionDescriptor.ControllerName))
                return true; // out of scope: the version tree doesn't make a determination about this action

            if (!(controllerActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is FingerprintAttribute) is FingerprintAttribute fingerprint))
                return true; // no identifier to make a determination

            if (!versionContext.Map.TryGetValue(controllerActionDescriptor.ControllerName, out var version))
                return true; // developer error?

            // finally we can determine if this is the intended version or not
            return version.Major == fingerprint.Major && version.Minor.GetValueOrDefault() == fingerprint.Minor;
        }
    }
}
