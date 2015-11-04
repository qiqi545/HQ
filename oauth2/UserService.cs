using System.Threading.Tasks;
using Microsoft.Framework.Caching.Distributed;
using oauth2.Models;
using oauth2.Security;

namespace gadfly.auth
{
    public class UserService
    {
        private const string UserPrefixKey = "auth::user::";

        private readonly ISecurityService _security;
        private readonly IUserRepository _users;
        private readonly IDistributedCache _cache; // must contain validated users only

        public UserService(ISecurityService security, IUserRepository users, IDistributedCache cache)
        {
            _security = security;
            _users = users;
            _cache = cache;
        }

        public async Task<IUser> GetValidatedAsync(string username, string password)
        {
            /*
            // Cache:
            //
            var user = _cache.GetAsync(UserPrefixKey + username);
            if (user != null)
            {
                return user;
            }
            */

            var user = await _users.GetByUsernameForValidationAsync(username);
            if (user == null)
                return null;

            if (!_security.ValidateHash(user.PasswordHash, _security.Hash(password)))
            {
                return null;
            }

            return user;
        }
    }
}