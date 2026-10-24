using CookingLah.F_Helpers;
using System;
using System.Web.UI;

namespace CookingLah.Pages.Admin
{
    public partial class AdminDashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Email"] == null || !Session["Role"].Equals("Admin"))
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
            }
            if (!IsPostBack)
            {
                // pre-load data
            }
        }
    }
}