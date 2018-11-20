namespace HQ.Rosetta.AspNetCore.Mvc
{
    public class OkErrorObjectResult<T> : ErrorResult
    {
        private readonly T _data;

        public OkErrorObjectResult(T data, Error error, params object[] arguments) : base(error, arguments)
        {
            _data = data;
        }

        protected override void FormatError()
        {
            if (Arguments.Length > 0)
                Error.Message = string.Format(Error.Message, Arguments);
            Value = new
            {
                data = _data,
                error = Error
            };
            StatusCode = 200;
        }
    }
}
