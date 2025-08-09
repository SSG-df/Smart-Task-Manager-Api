using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManager.DTOs.Auth;
using SmartTaskManager.Interfaces;

namespace SmartTaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (result == null)
                return BadRequest("Email or Username already taken");

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }

        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
        {
            var result = await _authService.CreateAdminAsync(dto);
            if (result == null)
                return BadRequest("Email or Username already taken");

            return Ok(result);
        }

        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userRole = User.FindFirst("role")?.Value;
            var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            Console.WriteLine($"User role: {userRole}");
            Console.WriteLine($"All claims: {string.Join(", ", allClaims)}");

            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpDelete("admin/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var success = await _authService.DeleteUserAsync(userId);
            if (!success)
                return BadRequest("Cannot delete user or user not found");

            return NoContent();
        }
    }
}
