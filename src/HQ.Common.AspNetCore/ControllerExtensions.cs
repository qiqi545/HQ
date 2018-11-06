using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Common.AspNetCore
{
	public static class ControllerExtensions
	{
		public static bool TryValidateModelState(this Controller controller, out IActionResult errorResult)
		{
			if (!controller.ModelState.IsValid)
			{
				errorResult = controller.BadRequestWithErrors();
				return false;
			}

			errorResult = null;
			return true;
		}

		public static BadRequestObjectResult BadRequestWithErrors(this ControllerBase controller)
		{
			return controller.BadRequest(
				controller.ModelState.Values
					.SelectMany(e => e.Errors)
					.Select(e => e.ErrorMessage));
		}

		public static InternalServerErrorObjectResult InternalServerError(this ControllerBase controller)
		{
			return new InternalServerErrorObjectResult(controller.ModelState.Values
				.SelectMany(e => e.Errors)
				.Select(e => e.ErrorMessage));
		}

		public static T TryOperation<T>(this Controller controller, string localizedErrorMessage, Func<T> operation, string action = "")
		{
			try
			{
				var result = operation();
				if (result is bool flag && !flag)
					controller.ModelState.TryAddModelError(action, localizedErrorMessage);
				return result;
			}
			catch (Exception ex)
			{
				controller.ModelState.TryAddModelException(action, ex);
				return default;
			}
		}

		public static Task<T> TryOperationAsync<T>(this Controller controller, string localizedErrorMessage, Task<T> operation, string action = "")
		{
			try
			{
				var result = operation.Result;
				if (result is bool flag && !flag)
					controller.ModelState.TryAddModelError(action, localizedErrorMessage);
				return Task.FromResult(result);
			}
			catch (Exception ex)
			{
				controller.ModelState.TryAddModelException(action, ex);
				return Task.FromResult(default(T));
			}
		}
	}
}