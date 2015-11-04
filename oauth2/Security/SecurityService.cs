using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;

namespace oauth2.Security
{
    public class SecurityService : ISecurityService
    {
        private const int SaltLength = 24;
        private const int HashLength = 24;
        private const int HashIterations = 1000;

        private const int IterationIndex = 0;
        private const int SaltIndex = 1;
        private const int HashIndex = 2;

        public string Hash(string input)
        {
            var salt = GetNonceBytes(SaltLength);
            var hash = Hash(input, salt, HashIterations, HashLength);
            var result = $"{HashIterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            return result;
        }

        public string GetNonce(int size = 32)
        {
            var buffer = GetNonceBytes(size);
            return Convert.ToBase64String(buffer);
        }

        public byte[] GetNonceBytes(int size)
        {
            using (var crypto = RandomNumberGenerator.Create())
            {
                var buffer = new byte[size];
                crypto.GetBytes(buffer);
                return buffer;
            }
        }

        public SecureString GetSecureNonce(int size = 32)
        {
            try
            {
                var nonce = GetNonce(size);
                var result = new SecureString();
                foreach (var v in nonce)
                    result.AppendChar(v);
                return result;
            }
            finally
            {
                GC.Collect(0);
            }
        }

        public byte[] GetSaltBytes()
        {
            return GetNonceBytes(SaltLength);
        }

        public string GetSalt()
        {
            return GetNonce(SaltLength);
        }

        public bool ValidateHash(string toTest, string hash)
        {
            char[] delimiter = { ':' };
            var split = hash.Split(delimiter);
            var iterations = int.Parse(split[IterationIndex]);
            var salt = Convert.FromBase64String(split[SaltIndex]);

            var encodedHash = split[HashIndex];
            if (!IsBase64String(encodedHash))
                return false;

            var hashBytes = Convert.FromBase64String(encodedHash);
            var testHashBytes = Hash(toTest, salt, iterations, hashBytes.Length);
            return SlowEquals(hashBytes, testHashBytes);
        }

        private static bool SlowEquals(IList<byte> left, IList<byte> right)
        {
            var diff = (uint)left.Count ^ (uint)right.Count;
            for (var i = 0; i < left.Count && i < right.Count; i++)
            {
                diff |= (uint)(left[i] ^ right[i]);
            }
            return diff == 0;
        }

        private static byte[] Hash(string input, byte[] salt, int iterations, int outputBytes)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(input, salt) { IterationCount = iterations };
            return pbkdf2.GetBytes(outputBytes);
        }

        private static bool IsBase64String(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length % 4 != 0
                || value.Contains(' ') || value.Contains('\t') || value.Contains('\r') || value.Contains('\n'))
                return false;
            var index = value.Length - 1;
            if (value[index] == '=')
                index--;
            if (value[index] == '=')
                index--;
            for (var i = 0; i <= index; i++)
                if (IsInvalid(value[i]))
                    return false;
            return true;
        }

        private static bool IsInvalid(char value)
        {
            var intValue = (int)value;
            if (intValue >= 48 && intValue <= 57)
                return false;
            if (intValue >= 65 && intValue <= 90)
                return false;
            if (intValue >= 97 && intValue <= 122)
                return false;
            return intValue != 43 && intValue != 47;
        }
    }
}
