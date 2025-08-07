using SmartTaskManager.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SmartTaskManager.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        public bool Verify(string password, string hash) =>
            Hash(password) == hash;
    }
}
