using System.Threading.Tasks;

namespace HQ.Platform.Security.Messaging
{
    public interface IEntropyProvider<in TSubject>
    {
        Task<string> GetValueAsync(TSubject subject, string modifier);
    }
}
