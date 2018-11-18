using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace HQ.Rosetta
{
    public interface IObjectGetRepository<T> where T : IObject
    {
        Task<Operation<IPage<T>>> GetAsync(SortOptions sort = null, PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);
        Task<Operation<T>> GetAsync(long id, FieldOptions fields = null, ProjectionOptions projection = null);
        Task<Operation<IPage<T>>> GetAsync(string query, SortOptions sort = null, PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);
    }

    public interface IObjectSaveRepository<in T> where T : IObject
    {
        Task<Operation<ObjectSave>> SaveAsync(T @object);
        Task<Operation<ObjectSave>> SaveAsync(T @object, DynamicObject partial);
        Task<Operation<IEnumerable<ObjectSave>>> SaveAsync(IEnumerable<T> objects, long startingAt = 0, int? count = null);
    }

    public interface IObjectDeleteRepository<in T> where T : IObject
    {
        Task<Operation<ObjectDelete>> DeleteAsync(long id);
        Task<Operation<ObjectDelete>> DeleteAsync(T @object);
        Task<Operation<IEnumerable<ObjectDelete>>> DeleteAsync(IEnumerable<long> ids, long startingAt = 0, int? count = null);
        Task<Operation<IEnumerable<ObjectDelete>>> DeleteAsync(IEnumerable<T> objects, long startingAt = 0, int? count = null);
    }
}
