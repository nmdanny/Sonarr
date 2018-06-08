using NLog;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NzbDrone.Core.TraktIntegration.Credentials
{
    /// <summary>
    /// Signs and verifies <see cref="OAuthState"/>
    /// </summary>
    public interface IOAuthStateCrypto
    {
        void Sign(OAuthState state);
        bool Verify(OAuthState state);
    }

    public class OAuthStateCrypto : IDisposable, IOAuthStateCrypto
    {
        private readonly Random random;
        private static readonly string hashAlg = "SHA512";
        // Static in order to share the key between all instances.
        private static readonly RSACryptoServiceProvider cryptoProvider = new RSACryptoServiceProvider();

        public OAuthStateCrypto(Random random, Logger logger)
        {
            this.random = random;
        }

        public void Dispose()
        {
            cryptoProvider.Dispose();
        }

        public void Sign(OAuthState state)
        {
            state.RandomBytes = new byte[32];
            random.NextBytes(state.RandomBytes);

            var formatter = new RSAPKCS1SignatureFormatter(cryptoProvider);
            formatter.SetHashAlgorithm(hashAlg);
            using (var sha512 = SHA512.Create())
            {
                var dataToSign = ToBytes(state);
                var hashedData = sha512.ComputeHash(dataToSign);
                state.Signature = formatter.CreateSignature(hashedData);
            }
        }

        public bool Verify(OAuthState state)
        {
            var deformatter = new RSAPKCS1SignatureDeformatter(cryptoProvider);
            deformatter.SetHashAlgorithm(hashAlg);
            using (var sha512 = SHA512.Create())
            {
                var dataToVerify = ToBytes(state);
                var hashedData = sha512.ComputeHash(dataToVerify);
                return deformatter.VerifySignature(hashedData, state.Signature);
            }
        }

        private byte[] ToBytes(OAuthState state)
        {
            var expectedBytes = Encoding.UTF8.GetBytes($"{state.ClientId}:{state.Secret}:{state.RedirectTo}");
            return expectedBytes.Concat(state.RandomBytes).ToArray();
        }
    }
}
