#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Security;
using System.Security.Cryptography;
using Sodium;

namespace HQ.Extensions.Cryptography.Internal
{
    /// <summary>
    ///     <see
    ///         href="https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.create?view=netframework-4.7.2" />
    /// </summary>
    internal static class Hashes
    {
        private const string Blake2BDefaultHashName = "Blake2b";

        static Hashes()
        {
            CryptoConfig.AddAlgorithm(typeof(GenericHash.GenericHashAlgorithm), Blake2BDefaultHashName, "Blake-2b");
        }

#if NETCOREAPP
        public static void ComputeHash(byte[] buffer, byte[] hash, HashType type, HashSource source)
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms && source != HashSource.SystemNetFips)
                throw new SecurityException("This environment restricts hash algorithms to only those that are FIPS certified.");

            switch (source)
            {
                case HashSource.SystemNetFips:
                    FipsHash(type, buffer, hash);
                    break;
                case HashSource.SodiumCore:
                    SodiumHash(type, buffer, hash);
                    break;
                case HashSource.NSec:
                    NSecHash(type, buffer, hash);
                    break;
                case HashSource.SystemNetManaged:
                    ManagedHash(type, buffer, hash);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
#endif

        public static byte[] ComputeHash(byte[] buffer, HashType type, HashSource source)
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms && source != HashSource.SystemNetFips)
            {
                throw new SecurityException(
                    "This environment restricts hash algorithms to only those that are FIPS certified.");
            }

            switch (source)
            {
                case HashSource.SystemNetFips:
                    return FipsHash(type, buffer);
                case HashSource.SodiumCore:
                    return SodiumHash(type, buffer);
                case HashSource.NSec:
                    return NSecHash(type, buffer);
                case HashSource.SystemNetManaged:
                    return ManagedHash(type, buffer);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        #region Factory

        private static readonly HashAlgorithm Md5Managed = MD5.Create();
        private static readonly HashAlgorithm Sha1Managed = SHA1.Create();
        private static readonly HashAlgorithm Sha256Managed = SHA256.Create();
        private static readonly HashAlgorithm Sha384Managed = SHA384.Create();
        private static readonly HashAlgorithm Sha512Managed = SHA512.Create();

        private static byte[] ManagedHash(HashType type, byte[] buffer)
        {
            switch (type)
            {
                case HashType.Md5:
                    return Md5Managed.ComputeHash(buffer);
                case HashType.Sha1:
                    return Sha1Managed.ComputeHash(buffer);
                case HashType.Sha256:
                    return Sha256Managed.ComputeHash(buffer);
                case HashType.Sha384:
                    return Sha384Managed.ComputeHash(buffer);
                case HashType.Sha512:
                    return Sha512Managed.ComputeHash(buffer);
                case HashType.SipHash24:
                case HashType.Blake2B:
                default:
                    throw new NotSupportedException();
            }
        }

        private static readonly HashAlgorithm Md5Csp = new MD5CryptoServiceProvider();
        private static readonly HashAlgorithm Sha1Csp = new SHA1CryptoServiceProvider();
        private static readonly HashAlgorithm Sha256Csp = new SHA256CryptoServiceProvider();
        private static readonly HashAlgorithm Sha384Csp = new SHA384CryptoServiceProvider();
        private static readonly HashAlgorithm Sha512Csp = new SHA512CryptoServiceProvider();

        private static byte[] FipsHash(HashType type, byte[] buffer)
        {
            switch (type)
            {
                case HashType.Md5:
                    return Md5Csp.ComputeHash(buffer);
                case HashType.Sha1:
                    return Sha1Csp.ComputeHash(buffer);
                case HashType.Sha256:
                    return Sha256Csp.ComputeHash(buffer);
                case HashType.Sha384:
                    return Sha384Csp.ComputeHash(buffer);
                case HashType.Sha512:
                    return Sha512Csp.ComputeHash(buffer);
                case HashType.SipHash24:
                case HashType.Blake2B:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static byte[] NSecHash(HashType type, byte[] buffer)
        {
            switch (type)
            {
                case HashType.Sha256:
                    return NSec.Cryptography.HashAlgorithm.Sha256.Hash(buffer);
                case HashType.Sha512:
                    return NSec.Cryptography.HashAlgorithm.Sha512.Hash(buffer);
                case HashType.Md5:
                case HashType.Sha1:
                case HashType.Sha384:
                case HashType.Blake2B:
                case HashType.SipHash24:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static readonly HashAlgorithm Blake2B = HashAlgorithm.Create(Blake2BDefaultHashName);
        private static readonly byte[] SipHash24Key = ShortHash.GenerateKey();

        private static byte[] SodiumHash(HashType hash, byte[] buffer)
        {
            switch (hash)
            {
                case HashType.Blake2B:
                    return Blake2B.ComputeHash(buffer);
                case HashType.SipHash24:
                    return ShortHash.Hash(buffer, SipHash24Key);
                case HashType.Sha256:
                    return CryptoHash.Sha256(buffer);
                case HashType.Sha512:
                    return CryptoHash.Sha512(buffer);
                case HashType.Md5:
                case HashType.Sha1:
                case HashType.Sha384:
                default:
                    throw new NotSupportedException();
            }
        }

#if NETCOREAPP
        private static void ManagedHash(HashType type, ReadOnlySpan<byte> buffer, Span<byte> destination)
        {
            TryComputeHash(type, buffer, destination, Md5Managed, Sha1Managed, Sha256Managed, Sha384Managed, Sha512Managed);
        }

        private static void FipsHash(HashType type, ReadOnlySpan<byte> buffer, Span<byte> destination)
        {
            TryComputeHash(type, buffer, destination, Md5Csp, Sha1Csp, Sha256Csp, Sha384Csp, Sha512Csp);
        }

        private static void NSecHash(HashType type, ReadOnlySpan<byte> buffer, Span<byte> hash)
        {
            switch (type)
            {
                case HashType.Sha256:
                    NSec.Cryptography.HashAlgorithm.Sha256.Hash(buffer, hash);
                    break;
                case HashType.Sha512:
                    NSec.Cryptography.HashAlgorithm.Sha512.Hash(buffer, hash);
                    break;
                case HashType.Md5:
                case HashType.Sha1:
                case HashType.Sha384:
                case HashType.Blake2B:
                case HashType.SipHash24:
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void SodiumHash(HashType type, byte[] buffer, byte[] hash)
        {
            switch (type)
            {
                case HashType.Blake2B:
                {
                    Blake2B.TryComputeHash(buffer, hash, out _);
                    break;
                }
                case HashType.SipHash24:
                {
                    SodiumLibrary.crypto_shorthash(hash, buffer, buffer.LongLength, SipHash24Key);
                    break;
                }
                case HashType.Sha256:
                {
                    SodiumLibrary.crypto_hash_sha256(hash, buffer, buffer.LongLength);
                    break;
                }
                case HashType.Sha512:
                {
                    SodiumLibrary.crypto_hash_sha512(hash, buffer, buffer.LongLength);
                    break;
                }
                case HashType.Md5:
                case HashType.Sha1:
                case HashType.Sha384:
                default:
                    throw new NotSupportedException();
            }
        }

        private static void TryComputeHash(HashType hash, ReadOnlySpan<byte> buffer, Span<byte> destination,
            HashAlgorithm md5, HashAlgorithm sha1, HashAlgorithm sha256, HashAlgorithm sha384, HashAlgorithm sha512)
        {
            switch (hash)
            {
                case HashType.Md5:
                    md5.TryComputeHash(buffer, destination, out _);
                    break;
                case HashType.Sha1:
                    sha1.TryComputeHash(buffer, destination, out _);
                    break;
                case HashType.Sha256:
                    sha256.TryComputeHash(buffer, destination, out _);
                    break;
                case HashType.Sha384:
                    sha384.TryComputeHash(buffer, destination, out _);
                    break;
                case HashType.Sha512:
                    sha512.TryComputeHash(buffer, destination, out _);
                    break;
                case HashType.SipHash24:
                case HashType.Blake2B:
                default:
                    throw new NotSupportedException();
            }
        }
#endif

        #endregion
    }
}
