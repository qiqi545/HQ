using System.Collections;

namespace HQ.Touchstone.Assertions
{
    public interface IAssert
    {
        void NotNull(object instance);
        void NotEmpty(IEnumerable enumerable);
        void True(bool condition);
    }
}
