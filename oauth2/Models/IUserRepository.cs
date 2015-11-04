using System.Collections.Generic;
using System.Threading.Tasks;

namespace oauth2.Models
{
    public interface IUserRepository
    {
        Task<IUser> GetByUsernameAsync(string username);
        Task<IAuthenticatingUser> GetByUsernameForValidationAsync(string username);
        Task<IEnumerable<string>> GetRolesAsync(IUser user);
    }
}