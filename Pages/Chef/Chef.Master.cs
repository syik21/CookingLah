using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Chef
{
    public partial class Chef : System.Web.UI.MasterPage
    {
        protected void Logout_Click(object sender, EventArgs e)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Response.Redirect(CookingLah.F_Helpers.NavigationHelper.GetUrl("Home"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}