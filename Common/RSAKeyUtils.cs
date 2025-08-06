using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Common
{
    public class RSAKeyUtils
    {
        public static RsaSecurityKey LoadRSAKey(string keyPath)
        {
            var privateKeyText = File.ReadAllText(keyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyText.ToCharArray());
            return new RsaSecurityKey(rsa);
        }
    }
}
