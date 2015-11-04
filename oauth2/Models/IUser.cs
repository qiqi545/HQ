using System.Security.Claims;

namespace oauth2.Models
{
    public interface IUser
    {
        ClaimsIdentity GetClaimsIdentity();
    }
}