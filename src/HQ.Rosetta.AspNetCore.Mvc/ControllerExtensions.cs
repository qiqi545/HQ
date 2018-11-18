using System.Collections.Generic;
using HQ.Common.AspNetCore.Mvc;

namespace HQ.Rosetta.AspNetCore.Mvc
{
    public static class ControllerExtensions
    {
        #region Validation

        public static Error ConvertModelStateToError(this ControllerExtended controller)
        {
            var errors = new List<Error>();
            foreach (var modelState in controller.ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    string message = !string.IsNullOrWhiteSpace(error.ErrorMessage) ? error.ErrorMessage : error.Exception.Message;
                    errors.Add(new Error(ErrorEvents.ValidationFailed, message, 422));
                }
            }
            var validationError = new Error(ErrorEvents.ValidationFailed, "Validation Failed", 422, errors);
            return validationError;
        }

        #endregion
    }
}
