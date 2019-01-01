using System.Collections.Generic;

namespace HQ.Rosetta
{
    public interface IValidated
    {
        bool TryValidate(out IList<Error> errors);
    }
}