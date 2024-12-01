using Microsoft.IdentityModel.Tokens;
using StoreRewards.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StoreRewards.Services
{

    public class AuthService
    {
        private readonly IConfiguration _configuration;


        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService( IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateToken(int userId,string email, List<string> roles)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_Key") ?? "JWT_Key"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // Store user ID
        };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT_Issuer"),
                audience: Environment.GetEnvironmentVariable("JWT_Audience"),
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(Environment.GetEnvironmentVariable("TokenExpiryHours"))),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public string CreateRandomToken(int tokenLength = 64)
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(tokenLength));
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public int GetUserId()
        {
            // Extract the user ID claim
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 0; 
            }

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return 0; 
        }


    }
}
