using System.Security.Cryptography;
using System.Text;

namespace WadiAlAin.Infrastructure.Authentication
{
    public static class PasswordHasher
    {
        private const string Salt = "w4t3r_A1n_s3cur1ty_2026";

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{Salt}:{password.Trim()}");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var computed = HashPassword(password);
            return string.Equals(computed, storedHash.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}