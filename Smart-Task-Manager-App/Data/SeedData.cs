using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Models;
using SmartTaskManager.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace SmartTaskManager.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(SmartTaskManagerDbContext context, IPasswordHasherService passwordHasher)
        {
            if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                return;
            }

            var defaultAdmin = new User
            {
                Username = "admin",
                Email = "admin@taskmanager.com",
                Role = UserRole.Admin,
                PasswordHash = passwordHasher.Hash("Admin123"),
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(defaultAdmin);
            await context.SaveChangesAsync();
        }
    }
}