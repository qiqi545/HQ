namespace HQ.Tokens.Configuration
{
    public class ClaimOptions
    {
        public string UserNameClaim { get; set; } = ClaimTypes.UserName;
        public string RoleClaim { get; set; } = ClaimTypes.Role;
        public string PermissionClaim { get; set; } = ClaimTypes.Permission;
    }
}