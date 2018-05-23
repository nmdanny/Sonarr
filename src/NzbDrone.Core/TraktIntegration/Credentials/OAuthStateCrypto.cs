using NLog;
using System;
using System.Security.Cryptography;

namespace NzbDrone.Core.TraktIntegration.Credentials
{
    /// <summary>
    /// Signs and verifies <see cref="OAuthState"/>
    /// </summary>
    public class OAuthStateCrypto : IDisposable
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
            state.RandomBytes = new byte[64];
            random.NextBytes(state.RandomBytes);
            var formatter = new RSAPKCS1SignatureFormatter(cryptoProvider);
            formatter.SetHashAlgorithm(hashAlg);
            state.Signature = formatter.CreateSignature(state.RandomBytes);
        }

        public bool Verify(OAuthState state)
        {
            var deformatter = new RSAPKCS1SignatureDeformatter(cryptoProvider);
            deformatter.SetHashAlgorithm(hashAlg);
            return deformatter.VerifySignature(state.RandomBytes, state.Signature);
        }
    }
}
