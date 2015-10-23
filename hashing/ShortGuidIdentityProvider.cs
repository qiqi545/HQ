namespace hashing
{
    public class ShortGuidIdentityProvider : IIdentityProvider
    {
        public object Short()
        {
            return ShortGuid.NewGuid();
        }
    }
}