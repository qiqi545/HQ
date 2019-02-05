//using System;
//using System.Net;
//using System.Threading.Tasks;
//using HQ.Platform.Security.Configuration;
//using Microsoft.Extensions.Options;
//using NSec.Cryptography;
//using Sodium;
//using HQ.Cryptography;

//namespace HQ.Platform.Security.Messaging
//{
//    public class MessageAuthenticationService<TSubject> where TSubject : class
//    {
//        private readonly IEntropyProvider<TSubject> _entropy;
//        private readonly IKeygenStore _store;
//        private readonly IOptions<MessageAuthenticationOptions> _options;

//        public MessageAuthenticationService(IEntropyProvider<TSubject> entropy, IKeygenStore store, IOptions<MessageAuthenticationOptions> options)
//        {
//            _entropy = entropy;
//            _store = store;
//            _options = options;
//        }

//        public async Task<string> GetOneTimePasswordAsync(TSubject subject, string modifier, TimeSpan? ttl = null)
//        {
//            var key = await _store.AcquireKeyAsync(KeyType.OneTimePassword);
//            var entropy = await _entropy.GetValueAsync(subject, modifier);
//            var message = $"{GetType()}:{modifier}:{entropy}";
//            var signature = OneTimeAuth.Sign(message, key);
//            return Utilities.BinaryToHex(signature, Utilities.HexFormat.None);
//        }

//        public async Task<string> GetMessageAuthenticationCodeAsync(TSubject subject, string modifier, TimeSpan? ttl = null)
//        {
//            var nonce = await _store.AcquireNonceAsync();
//            var entropy = await _entropy.GetValueAsync(subject, modifier);
//            var message = $"{GetType()}:{modifier}:{entropy}";

//            var signature = SecretBox.Create(message, key, key);
//            return Utilities.BinaryToHex(signature, Utilities.HexFormat.None);
//        }

//        internal static int ComputeHotp(HashAlgorithm hashAlgorithm, ulong timeStep, string modifier)
//        {
//            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timeStep));
//            byte[] hash = hashAlgorithm.Hash(bytes.Concat(modifier, MessageEncoding.current));
//            int index = (int)hash[hash.Length - 1] & 15;
//            return (((int)hash[index] & (int)sbyte.MaxValue) << 24 | ((int)hash[index + 1] & (int)byte.MaxValue) << 16 | ((int)hash[index + 2] & (int)byte.MaxValue) << 8 | (int)hash[index + 3] & (int)byte.MaxValue) % 1000000;
//        }
        
//        internal static int ComputeTotp(HashAlgorithm hashAlgorithm, ulong timeStep, string modifier)
//        {
//            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timeStep));
//            byte[] hash = hashAlgorithm.Hash(bytes.Concat(modifier, MessageEncoding.current));
//            int index = (int)hash[hash.Length - 1] & 15;
//            return (((int)hash[index] & (int)sbyte.MaxValue) << 24 | ((int)hash[index + 1] & (int)byte.MaxValue) << 16 | ((int)hash[index + 2] & (int)byte.MaxValue) << 8 | (int)hash[index + 3] & (int)byte.MaxValue) % 1000000;
//        }


        
//    }
//}
