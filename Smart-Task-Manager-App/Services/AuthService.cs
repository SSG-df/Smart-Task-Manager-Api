using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManager.Data;
using SmartTaskManager.DTOs.Auth;
using SmartTaskManager.Interfaces;
using SmartTaskManager.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTaskManager.Services
{
    public class AuthService : IAuthService
    {
        private readonly SmartTaskManagerDbContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasherService _hasher;

        public AuthService(
            SmartTaskManagerDbContext context,
            IConfiguration config,
            IPasswordHasherService hasher)
        {
            _context = context;
            _config = config;
            _hasher = hasher;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Email cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                throw new ArgumentException("Username cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Password cannot be empty");
            }

            bool userExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email.Trim() || u.Username == dto.Username.Trim());

            if (userExists)
            {
                return null;
            }

            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Role = UserRole.RegularUser,
                PasswordHash = _hasher.Hash(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return GenerateToken(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Email cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Password cannot be empty");
            }

            var normalizedEmail = dto.Email.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !_hasher.Verify(dto.Password, user.PasswordHash))
            {
                return null;
            }

            return GenerateToken(user);
        }

        public async Task<AuthResponseDto?> CreateAdminAsync(CreateAdminDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Email cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                throw new ArgumentException("Username cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Password cannot be empty");
            }

            bool userExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email.Trim() || u.Username == dto.Username.Trim());

            if (userExists)
            {
                return null;
            }

            var admin = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Role = UserRole.Admin,
                PasswordHash = _hasher.Hash(dto.Password)
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return GenerateToken(admin);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username!,
                    Email = u.Email!,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                if (adminCount <= 1)
                {
                    return false;
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private AuthResponseDto GenerateToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email cannot be empty");
            }

            var jwtKey = _config["JwtSettings:Key"];
            var jwtIssuer = _config["JwtSettings:Issuer"];
            var jwtAudience = _config["JwtSettings:Audience"];
            var jwtExpires = _config["JwtSettings:ExpiresInMinutes"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("JwtSettings:Key configuration is missing");
            }

            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new ArgumentNullException("JwtSettings:Issuer configuration is missing");
            }

            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new ArgumentNullException("JwtSettings:Audience configuration is missing");
            }

            if (string.IsNullOrEmpty(jwtExpires))
            {
                throw new ArgumentNullException("JwtSettings:ExpiresInMinutes configuration is missing");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("role", user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtExpires));

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}