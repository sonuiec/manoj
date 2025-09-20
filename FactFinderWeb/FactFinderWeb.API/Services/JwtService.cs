
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;

namespace FactFinderWeb.API.Services
{
    public class JwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireMinutes;

        public JwtService(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            _expireMinutes = int.Parse(configuration["Jwt:ExpireMinutes"]);
        }

        public string GenerateToken(string userId, string username, string IsAdmin, string IsCaregiver, string role,string CaregiverDB)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            var claims = new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", userId),
                new Claim("GivenName", username),
                new Claim("IsAdmin", IsAdmin),
                new Claim("IsCaregiver", IsCaregiver),
                 new Claim("CaregiverDB", CaregiverDB),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expireMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static TokenModel GetUserFromToken(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;

            if (identity == null || !identity.Claims.Any())
                return null; // No valid token found

            return new TokenModel
            {
                UserId = Convert.ToInt32(identity.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value),
                IsCaregiver = Convert.ToBoolean(identity.Claims.FirstOrDefault(c => c.Type == "IsCaregiver")?.Value),
                IsAdmin = Convert.ToBoolean(identity.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value),
                Role = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
                GivenName = identity.Claims.FirstOrDefault(c => c.Type == "GivenName")?.Value,
                CaregiverDB = Convert.ToBoolean(identity.Claims.FirstOrDefault(c => c.Type == "CaregiverDB")?.Value),
            };
        }

        public static TokenModel ParseToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                if (jwtToken == null)
                    return null;

                var claims = jwtToken.Claims;

                return new TokenModel
                {
                    UserId = Convert.ToInt32( claims.FirstOrDefault(c => c.Type == "UserId")?.Value),
                    GivenName = claims.FirstOrDefault(c => c.Type == "GivenName")?.Value,
                    Role = claims.FirstOrDefault(c => c.Type == "role")?.Value,
                    IsAdmin = bool.TryParse(claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value, out bool isAdmin) && isAdmin,
                    IsCaregiver = bool.TryParse(claims.FirstOrDefault(c => c.Type == "IsCaregiver")?.Value, out bool isCaregiver) && isCaregiver,
                    Expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtToken.Claims.First(c => c.Type == "exp").Value)).UtcDateTime
                };
            }
            catch
            {
                return null;
            }
        }

    
    }
}
