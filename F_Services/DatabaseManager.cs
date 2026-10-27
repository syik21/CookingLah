using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;

namespace CookingLah.F_Services
{
    public class DatabaseManager
    {
        public static SqlConnection ConnectDB()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["CookinglahDB"].ConnectionString);
        }

        // Load the latest username and profile photo for a master-page header.
        public static bool TryLoadMasterProfile(object userIdValue, out string username, out string profileImage)
        {
            username = "";
            profileImage = "~/F_Assets/mascot.png";

            int userId;
            if (!int.TryParse(Convert.ToString(userIdValue), out userId) || userId <= 0) return false;

            using (SqlConnection connection = ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT Username, ProfileImage
                FROM Users
                WHERE UserID=@userId
                  AND AccountStatus='Active'", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return false;

                    username = Convert.ToString(reader["Username"]);
                    profileImage = SafeProfileImage(Convert.ToString(reader["ProfileImage"]));
                    return true;
                }
            }
        }

        private static string SafeProfileImage(string path)
        {
            path = (path ?? "").Trim();
            if (path.Length == 0 || path.StartsWith("//") || path.Contains("\\")) return "~/F_Assets/mascot.png";

            Uri absoluteUrl;
            if (Uri.TryCreate(path, UriKind.Absolute, out absoluteUrl) &&
                absoluteUrl.Scheme != Uri.UriSchemeHttp &&
                absoluteUrl.Scheme != Uri.UriSchemeHttps)
            {
                return "~/F_Assets/mascot.png";
            }

            return path;
        }
    }
}
