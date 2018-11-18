using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Rosetta.AspNetCore.Mvc
{
	public class ErrorResult : ObjectResult
	{
		private readonly Error _error;
		private readonly object[] _args;

		public ErrorResult(Error error, params object[] args) : base(error)
		{
			_error = error;
			_args = args;
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

		private void FormatError()
		{
			if (_args.Length > 0)
				_error.Message = string.Format(_error.Message, _args);
			Value = _error;
			StatusCode = _error.StatusCode;
		}
	}
}
