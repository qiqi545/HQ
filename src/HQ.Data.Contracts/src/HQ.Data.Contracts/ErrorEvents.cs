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

namespace HQ.Data.Contracts
{
    public static class ErrorEvents
    {
        /// <summary>
        /// The request was improperly structured, to the point it could not be validated.
        /// </summary>
        public const long InvalidRequest = 1001;

        /// <summary>
        /// The request was evaluated, but failed validation
        /// </summary>
        public const long ValidationFailed = 1001;

        /// <summary>
        /// The request is valid, but could expose sensitive data.
        /// </summary>
        public const long UnsafeRequest = 1002;


        public const long FieldDoesNotMatch = 1003;
        public const long AggregateErrors = 1004;
        public const long IdentityError = 1005;
        public const long ResourceMissing = 1006;
        public const long ResourceNotImplemented = 1007;
        public const long InvalidParameter = 1008;
        public const long RequestEntityTooLarge = 1009;
        public const long CouldNotAcceptWork = 1011;
    }
}
