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

using HQ.Common.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Data.Contracts.AspNetCore.Mvc
{
    public class DataController : ControllerExtended
    {
        [ApiExplorerSettings(IgnoreApi = true), NonAction]
        public IActionResult Error(Error error, params object[] args)
        {
            return new ErrorResult(error, args);
        }

        [ApiExplorerSettings(IgnoreApi = true), NonAction]
        public bool Valid(object model, out ErrorResult error, params object[] args)
        {
            if (!TryValidateModel(model))
            {
                var validationError = ConvertModelStateToError();
                error = new ErrorResult(validationError);
                return false;
            }

            error = null;
            return true;
        }

        [ApiExplorerSettings(IgnoreApi = true), NonAction]
        public bool ValidModelState(out ErrorResult error, params object[] args)
        {
            if (!TryValidateModel(ModelState))
            {
                var validationError = ConvertModelStateToError();
                error = new ErrorResult(validationError);
                return false;
            }

            error = null;
            return true;
        }

        [ApiExplorerSettings(IgnoreApi = true), NonAction]
        public Error ConvertModelStateToError()
        {
            return ControllerExtensions.ConvertModelStateToError(this);
        }
    }
}
