using HQ.Common.AspNetCore.Mvc;

namespace HQ.Rosetta.AspNetCore.Mvc
{
    public class DataController : ControllerExtended
    {
        public bool Valid(object model, out ErrorResult error, params object[] args)
        {
            if (!TryValidateModel(model))
            {
                var validationError = ConvertModelStateToError();
                error = new ErrorResult(validationError);
                return false;
            }
            error = null;
            return true;
        }

        public Error ConvertModelStateToError()
        {
            return ControllerExtensions.ConvertModelStateToError(this);
        }
    }
}
