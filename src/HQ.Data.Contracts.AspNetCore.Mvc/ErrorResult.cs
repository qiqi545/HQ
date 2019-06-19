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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Data.Contracts.AspNetCore.Mvc
{
    public class ErrorResult : ObjectResult
    {
        protected readonly object[] Arguments;
        protected readonly Error Error;

        public ErrorResult(Error error, params object[] args) : base(error)
        {
            Error = error;
            Arguments = args;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            FormatError();
            await base.ExecuteResultAsync(context);
        }

        public override void ExecuteResult(ActionContext context)
        {
            FormatError();
            base.ExecuteResult(context);
        }

        protected virtual void FormatError()
        {
            if (Arguments.Length > 0)
            {
                Error.Message = string.Format(Error.Message, Arguments);
            }

            Value = Error;
            StatusCode = Error.StatusCode;
        }
    }
}
