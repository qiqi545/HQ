namespace oauth2.Models
{
    public interface IAuthenticatingUser : IUser
    {
        string PasswordHash { get; }
    }
}