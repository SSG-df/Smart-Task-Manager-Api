using SmartTaskManager.DTOs.Auth;

namespace SmartTaskManager.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<AuthResponseDto?> CreateAdminAsync(CreateAdminDto dto);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(int userId);
    }
}
