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
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Common.AspNetCore.Mvc
{
    public class ControllerExtended : Controller
    {
        #region Additional IActionResult Helpers

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult NotModified()
        {
            return StatusCode(HttpStatusCode.NotModified);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult Gone()
        {
            return StatusCode(HttpStatusCode.Gone);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult UnsupportedMediaType(object value = null)
        {
            return StatusCode(HttpStatusCode.UnsupportedMediaType, value);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult SeeOther(string location)
        {
            Response.Headers[Constants.HttpHeaders.Location] = location;
            return StatusCode(HttpStatusCode.SeeOther);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult Created(string location)
        {
            Response.Headers[Constants.HttpHeaders.Location] = location;
            return StatusCode(HttpStatusCode.Created);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult NotAcceptable()
        {
            return StatusCode(HttpStatusCode.NotAcceptable);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult NotImplemented()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult StatusCode(HttpStatusCode code)
        {
            return StatusCode((int)code);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult StatusCode(HttpStatusCode code, object value)
        {
            return StatusCode((int)code, value);
        }

        #endregion

        #region Dispatch

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public T TryOperation<T>(string localizedErrorMessage, Func<T> operation, string action = "")
        {
            try
            {
                var result = operation();
                if (result is bool flag && !flag)
                    ModelState.TryAddModelError(action, localizedErrorMessage);
                return result;
            }
            catch (Exception ex)
            {
                ModelState.TryAddModelException(action, ex);
                return default;
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public Task<T> TryOperationAsync<T>(string localizedErrorMessage, Task<T> operation, string action = "")
        {
            try
            {
                var result = operation.Result;
                if (result is bool flag && !flag)
                    ModelState.TryAddModelError(action, localizedErrorMessage);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                ModelState.TryAddModelException(action, ex);
                return Task.FromResult(default(T));
            }
        }

        #endregion
    }
}
