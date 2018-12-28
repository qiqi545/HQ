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
using System.Net;
using System.Runtime.Serialization;
using HQ.Common.Extensions;

namespace HQ.Rosetta
{
    [DataContract]
    public class Error
    {
        public Error(long eventId, string message, HttpStatusCode statusCode, IList<Error> errors = null) : this(
            eventId, message, (short)statusCode, errors)
        {
        }

        public Error(long eventId, string message, short statusCode = (short)HttpStatusCode.InternalServerError,
            params Error[] errors) : this(eventId, message, statusCode, (IList<Error>)errors)
        {
            Message = message;
            StatusCode = statusCode;
            Errors = errors;
        }

        public Error(long eventId, string message, short statusCode = (short)HttpStatusCode.InternalServerError, IEnumerable<Error> errors = null)
        {
            EventId = eventId;
            Errors = errors.AsList();
            StatusCode = statusCode;
            Message = message;
        }

        [DataMember]
        public short StatusCode { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public long EventId { get; set; }

        [DataMember]
        public IList<Error> Errors { get; set; }
    }
}
