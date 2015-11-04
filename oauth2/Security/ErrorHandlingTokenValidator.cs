using System.IdentityModel.Tokens;
using System.Security.Claims;

namespace oauth2.Security
{
    /// <summary>
    /// Workaround for an invalid token throwing an exception rather than surfacing as 401 Unauthorized
    /// </summary>
    public class ErrorHandlingTokenValidator : ISecurityTokenValidator
    {
        private readonly ISecurityTokenValidator _validator;

        public ErrorHandlingTokenValidator(ISecurityTokenValidator validator)
        {
            _validator = validator;
        }

        public bool CanReadToken(string securityToken)
        {
            return _validator.CanReadToken(securityToken);
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            try
            {
                return _validator.ValidateToken(securityToken, validationParameters, out validatedToken);
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                validatedToken = null;
                return null;
            }
        }

        public bool CanValidateToken => _validator.CanValidateToken;

        public int MaximumTokenSizeInBytes
        {
            get { return _validator.MaximumTokenSizeInBytes; }
            set { _validator.MaximumTokenSizeInBytes = value; }
        }
    }
}