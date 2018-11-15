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
using System.Linq;

namespace HQ.Rosetta
{
    public class Operation
    {
        public Operation()
        {
            Result = OperationResult.Succeeded;
        }

        public Operation(IEnumerable<Error> errors) : this()
        {
            Errors = errors;
        }

        public OperationResult Result { get; set; }
        public bool Succeeded => Result == OperationResult.Succeeded || Result == OperationResult.SucceededWithErrors;
        public bool HasErrors => Errors?.Count() > 0;
        public IEnumerable<Error> Errors { get; set; }

        public static Operation FromResult<T>(T data)
        {
            return new Operation<T>(data);
        }

        public static Operation FromResult<T>(T data, IEnumerable<Error> errors)
        {
            return new Operation<T>(data, errors);
        }
    }
}
