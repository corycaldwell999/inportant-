using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Services
{
    /// <summary>
    /// Password hashing service using BCrypt
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hash a plain text password
        /// </summary>
        string HashPassword(string password, int workFactor = 12);

        /// <summary>
        /// Verify a password against a hash
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }

    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password, int workFactor = 12)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }

    /// <summary>
    /// JWT token generation and validation service
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate an access token
        /// </summary>
        string GenerateAccessToken(Guid userId, string email, string displayName, string[] roles);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate a token and extract claims
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly string _signingKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtService(IConfiguration configuration)
        {
            _signingKey = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("JWT signing key not configured");
            _issuer = configuration["Jwt:Issuer"] ?? "https://localhost:7001";
            _audience = configuration["Jwt:Audience"] ?? "trauma-platform";
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

            // Validate key length
            if (_signingKey.Length < 32)
            {
                throw new InvalidOperationException("JWT signing key must be at least 32 characters");
            }
        }

        public string GenerateAccessToken(Guid userId, string email, string displayName, string[] roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_signingKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, displayName),
                new Claim("sub", userId.ToString()),
            };

            // Add roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_signingKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
