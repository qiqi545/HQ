using System.Collections.Generic;

namespace HQ.Data.Contracts
{
    public interface IValidated
    {
        bool TryValidate(out IList<Error> errors);
    }
}