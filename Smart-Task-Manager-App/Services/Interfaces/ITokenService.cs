using SmartTaskManager.Models;
using System.Security.Claims;

namespace SmartTaskManager.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        RefreshToken GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> RevokeRefreshToken(string token);
        Task<bool> SaveRefreshToken(int userId, string token);
    }
}