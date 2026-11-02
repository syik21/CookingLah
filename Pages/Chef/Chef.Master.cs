using System;
using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System.Web.UI;

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
            if (Session["Username"] == null)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            LoadHeaderProfile();
            base.OnPreRender(e);
        }

        // Refresh the header after profile changes are saved.
        private void LoadHeaderProfile()
        {
            string username;
            string profileImage;
            if (!DatabaseManager.TryLoadMasterProfile(Session["UserId"], out username, out profileImage))
                return;

            Session["Username"] = username;
            lblProfileName.Text = username;
            imgLearnerProfile.ImageUrl = profileImage;
        }
    }
}
