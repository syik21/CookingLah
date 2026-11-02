using System;
using System.Security.Cryptography;
using System.Text;

namespace CookingLah.F_Services
{
    public static class PasswordResetTokenManager
    {
        // Generate a secure one-time password reset token.
        public static string GenerateResetToken()
        {
            var bytes = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        // Hash the reset token before storing or validating it.
        public static string HashToken(string token)
        {
            using (var hash = SHA256.Create())
                return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(token)));
        }
    }
}
