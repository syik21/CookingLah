using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CookingLah.F_Services
{
    public class DatabaseManager
    {
        public static SqlConnection ConnectDB()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["CookinglahDB"].ConnectionString);
        }
    }
}