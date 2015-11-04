using System.Security.Claims;

namespace oauth2.Models
{
    public class AuthenticatingUser : IAuthenticatingUser
    {
        public ClaimsIdentity GetClaimsIdentity()
        {
            return null;
        }

        public string PasswordHash { get; set; }
    }
}