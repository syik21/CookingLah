using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CookingLah.F_Services
{
    public class UserManager
    {
        public static bool isAccActive(string status)
        {
            if (status == "Active")
            {
                return true;
            }
            return false;
        }

        public static bool IsEmail(string s)
        {
            return s.Length <= 256 && Regex.IsMatch(s, @"^\S+@\S+\.\S+$");
        }
    }
}