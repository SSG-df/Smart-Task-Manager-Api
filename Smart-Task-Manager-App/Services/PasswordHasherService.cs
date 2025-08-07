using SmartTaskManager.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SmartTaskManager.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            var builder = new StringBuilder(bytes.Length * 2);
            
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            
            return builder.ToString();
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
                
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));

            return Hash(password).Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}