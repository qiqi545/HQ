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
using System.Runtime.Serialization;

namespace HQ.Data.Contracts
{
    [DataContract]
    public class Operation
    {
        public Operation()
        {
            Result = OperationResult.Succeeded;
        }

        public Operation(Error error)
        {
	        Result = OperationResult.Error;
	        Errors = new List<Error>(error == null ? Enumerable.Empty<Error>() : new[] {error});
        }

        public Operation(ICollection<Error> errors) : this()
        {
            Result = errors?.Count > 0 ? OperationResult.Error : OperationResult.Succeeded;
            Errors = new List<Error>(errors ?? Enumerable.Empty<Error>());
        }

        public static Operation CompletedWithoutErrors => new Operation();

        [DataMember]
        public OperationResult Result { get; set; }

        [DataMember]
        public bool Succeeded => Result == OperationResult.Succeeded || Result == OperationResult.SucceededWithErrors;

        [DataMember]
        public bool HasErrors => Errors?.Count > 0;

        [DataMember]
        public IList<Error> Errors { get; private set; }

        public static Operation<T> FromResult<T>(T data)
        {
            return new Operation<T>(data);
        }

        public static Operation<T> FromResult<T>(T data, IList<Error> errors)
        {
            return new Operation<T>(data, errors);
        }
    }
}
