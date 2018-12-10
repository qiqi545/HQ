using System.Threading.Tasks;
using HQ.Cohort.Models;
using HQ.Rosetta;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Services
{
    public class SignInService<TUser> : ISignInService<TUser> where TUser : IdentityUserExtended
    {
        private readonly SignInManager<TUser> _signInManager;

        public SignInService(SignInManager<TUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<Operation> SignIn(TUser user, bool isPersistent, string authenticationMethod = null)
        {
            await _signInManager.SignInAsync(user, isPersistent, authenticationMethod);
            return Operation.CompletedWithoutErrors;
        }

        public async Task<Operation> SignOut()
        {
            /*
            await _signInManager.Context.SignOutAsync(IdentityConstants.ApplicationScheme);
            await _signInManager.Context.SignOutAsync(IdentityConstants.ExternalScheme);
            await _signInManager.Context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
            */

            await _signInManager.SignOutAsync();
            return Operation.CompletedWithoutErrors;
        }
    }
}
