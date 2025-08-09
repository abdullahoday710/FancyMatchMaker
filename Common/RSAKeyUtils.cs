using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Common
{
    public class RSAKeyUtils
    {
        public static string GetPublicKeyPath()
        {
            var publicKeyPath = Environment.GetEnvironmentVariable("ROCK_JWT_PUBLIC_KEY_PATH");

            if (publicKeyPath == null)
            {
                publicKeyPath = "../debugkeys/public.key";
            }

            return publicKeyPath;
        }

        public static string GetPrivateKeyPath()
        {
            var publicKeyPath = Environment.GetEnvironmentVariable("ROCK_JWT_PRIVATE_KEY_PATH");

            if (publicKeyPath == null)
            {
                publicKeyPath = "../debugkeys/private.key";
            }

            return publicKeyPath;
        }

        public static RsaSecurityKey LoadRSAKey(string keyPath)
        {
            var privateKeyText = File.ReadAllText(keyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyText.ToCharArray());
            return new RsaSecurityKey(rsa);
        }
    }
}
