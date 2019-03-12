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
using FastMember;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Extensions
{
    internal static class IdentityResultFactory
    {
        /// <summary>
        ///     Provides an <see cref="IdentityResult" /> in a failed state, with errors.
        ///     This prevents an allocation by forcing the error collection to an array.
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static IdentityResult Failed(ICollection<IdentityError> errors)
        {
            var accessor = TypeAccessor.Create(typeof(IdentityResult), true);
            var result = new IdentityResult();
            var list = accessor[result, "_errors"] as List<IdentityError>;
            list?.AddRange(errors);
            accessor[result, "Succeeded"] = false;
            return result;
        }
    }
}
