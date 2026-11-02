using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BCrypt.Net;

namespace CookingLah.F_Services
{
    public class PasswordManager
    {
        public static bool MeetsPasswordPolicy(string password)
        {
            return password != null && password.Length >= 8 &&
                System.Text.Encoding.UTF8.GetByteCount(password) <= 72 &&
                System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Za-z]") &&
                System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]") &&
                System.Text.RegularExpressions.Regex.IsMatch(password, @"[^\w\s]|_");
        }

        public static string HashPassword(string plain)
        {
            string hashed = BCrypt.Net.BCrypt.HashPassword(plain);
            return hashed;
        }
        public static bool isPasswordValid(string enterPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enterPassword, storedPassword);
        }
    }
}