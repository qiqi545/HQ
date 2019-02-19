using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HQ.Platform.Security.Messaging
{
    internal class DefaultEntropyProvider<TSubject> : IEntropyProvider<TSubject>
    {
        public Task<string> GetValueAsync(TSubject subject, string modifier)
        {
            return Task.FromResult(RuntimeHelpers.GetHashCode(subject) + modifier);
        }
    }
}