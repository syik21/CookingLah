using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CookingLah.F_Helpers
{
    public class NavigationHelper
    {
        private static readonly Dictionary<string, string> NavUrl =
            new Dictionary<string, string>
            {
                { "Home", "~/Main.aspx" },
                { "Login", "~/Pages/Shared/LoginSignupPage.aspx" },
                { "Instructor", "~/Pages/Chef/ChefDashboard.aspx" },
                { "Learner", "~/Pages/Learner/LearnerDashboard.aspx" },
            };

        public static string GetUrl(string pageName)
        {
            if (NavUrl.TryGetValue(pageName, out string url))
            {
                return url;
            }

            return "~/Main.aspx";
        }
    }
}