using Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.JWT
{
    public class JWTUtils
    {
        public static string GenerateJwtToken(string userId, string userEmail, DateTime expireationDate)
        {
            var privateKeyPath = RSAKeyUtils.GetPrivateKeyPath();

            RsaSecurityKey privateKey;

            if (privateKeyPath != null)
            {
                privateKey = RSAKeyUtils.LoadRSAKey(privateKeyPath);
            }
            else
            {
                privateKey = RSAKeyUtils.LoadRSAKey("secrets/private.key");
            }

            var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(JwtRegisteredClaimNames.Email, userEmail),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: "RockPaperScissorsAuthService",
                claims: claims,
                expires: expireationDate,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
