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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HQ.Common.AspNetCore
{
    /// <summary>
    ///     An <see cref="ObjectResult" /> that when executed will produce an Internal Server Error (500) response.
    /// </summary>
    public class InternalServerErrorObjectResult : ObjectResult
    {
        /// <summary>
        ///     Creates a new <see cref="InternalServerErrorObjectResult" /> instance.
        /// </summary>
        /// <param name="error">Contains the errors to be returned to the client.</param>
        public InternalServerErrorObjectResult(object error)
            : base(error)
        {
            StatusCode = (int) HttpStatusCode.InternalServerError;
        }

        /// <summary>
        ///     Creates a new <see cref="InternalServerErrorObjectResult" /> instance.
        /// </summary>
        /// <param name="modelState"><see cref="ModelStateDictionary" /> containing the validation errors.</param>
        public InternalServerErrorObjectResult(ModelStateDictionary modelState)
            : base(new SerializableError(modelState))
        {
            if (modelState == null)
                throw new ArgumentNullException(nameof(modelState));
            StatusCode = (int) HttpStatusCode.InternalServerError;
        }
    }
}
