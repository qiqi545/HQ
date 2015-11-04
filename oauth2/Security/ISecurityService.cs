using System.Security;

namespace oauth2.Security
{
    public interface ISecurityService
    {
        string Hash(string input);
        string GetNonce(int size = 32);
        byte[] GetNonceBytes(int size = 32);
        SecureString GetSecureNonce(int size = 32);
        string GetSalt();
        byte[] GetSaltBytes();
        bool ValidateHash(string toTest, string hash);
    }
}