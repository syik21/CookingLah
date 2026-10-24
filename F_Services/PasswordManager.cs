using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BCrypt.Net;

namespace CookingLah.F_Services
{
    public class PasswordManager
    {
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