using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Rosetta.AspNetCore.Mvc
{
    public class ErrorResult : ObjectResult
    {
        protected readonly Error Error;
        protected readonly object[] Arguments;

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
                Error.Message = string.Format(Error.Message, Arguments);
            Value = Error;
            StatusCode = Error.StatusCode;
        }
    }
}
