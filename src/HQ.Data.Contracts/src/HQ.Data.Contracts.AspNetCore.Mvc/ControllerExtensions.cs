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

using System.Collections.Generic;
using HQ.Common.AspNetCore.Mvc;
using HQ.Strings;

namespace HQ.Data.Contracts.AspNetCore.Mvc
{
    public static class ControllerExtensions
    {
        #region Validation

        public static Error ConvertModelStateToError(this ControllerExtended controller)
        {
            var errors = new List<Error>();
            foreach (var modelState in controller.ModelState.Values)
            foreach (var error in modelState.Errors)
            {
                var message = !string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? error.ErrorMessage
                    : error.Exception.Message;
                errors.Add(new Error(ErrorEvents.ValidationFailed, message, 422));
            }

            var validationError = new Error(ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed, 422,
                errors);
            return validationError;
        }

        #endregion
    }
}
