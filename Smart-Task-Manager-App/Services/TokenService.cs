using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManager.Data;
using SmartTaskManager.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SmartTaskManager.Services
{
    public class TokenService : ITokenService
    {
        private readonly SmartTaskManagerDbContext _context;
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            SmartTaskManagerDbContext context, 
            IConfiguration config,
            ILogger<TokenService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
            
            var jwtKey = _config["JwtSettings:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                throw new ArgumentException("JWT Key must be at least 32 characters long");
            
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

       public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"],
                Claims = {
                    [JwtRegisteredClaimNames.Sub] = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                    [JwtRegisteredClaimNames.UniqueName] = claims.First(c => c.Type == ClaimTypes.Name).Value,
                    [ClaimTypes.Role] = claims.First(c => c.Type == ClaimTypes.Role).Value
                }
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(_config["JwtSettings:RefreshTokenExpiresInDays"] ?? "7")),
                Created = DateTime.UtcNow
            };
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate token");
                throw;
            }
        }

        public async Task<bool> RevokeRefreshToken(string token)
        {
            try
            {
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token);

                if (refreshToken == null) 
                    return false;

                refreshToken.Revoked = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke refresh token");
                throw;
            }
        }

        public async Task<bool> SaveRefreshToken(int userId, string token)
        {
            try
            {
                var refreshToken = new RefreshToken
                {
                    Token = token,
                    UserId = userId,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(
                        Convert.ToDouble(_config["JwtSettings:RefreshTokenExpiresInDays"] ?? "7"))
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save refresh token");
                throw;
            }
        }

        private string GetClaimValue(IEnumerable<Claim> claims, string claimType)
        {
            return claims.FirstOrDefault(c => c.Type == claimType)?.Value 
                ?? throw new ArgumentException($"Missing required claim: {claimType}");
        }
    }
}