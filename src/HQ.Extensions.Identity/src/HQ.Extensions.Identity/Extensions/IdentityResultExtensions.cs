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
using HQ.Data.Contracts;
using Microsoft.AspNetCore.Identity;

namespace HQ.Extensions.Identity.Extensions
{
    public static class IdentityResultExtensions
    {
        public static Operation ToOperation(this IdentityResult result)
        {
            var errors = result.Errors.Select(x => new Error(ErrorEvents.IdentityError, $"{x.Code} - {x.Description}"));
            var operation = new Operation(errors.ToList());
            if (result.Succeeded)
                operation.Result =
                    operation.HasErrors ? OperationResult.SucceededWithErrors : OperationResult.Succeeded;
            else
                operation.Result = OperationResult.Error;
            return operation;
        }

        public static Operation<T> ToOperation<T>(this IdentityResult result, T data)
        {
            var errors = result.Errors.Select(x => new Error(ErrorEvents.IdentityError, $"{x.Code} - {x.Description}")).ToList();
            var operation = new Operation<T>(data, errors);
            if (result.Succeeded)
                operation.Result =
                    operation.HasErrors ? OperationResult.SucceededWithErrors : OperationResult.Succeeded;
            else
                operation.Result = OperationResult.Error;
            return operation;
        }
    }
}
