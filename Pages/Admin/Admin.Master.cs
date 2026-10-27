using CookingLah.F_Helpers;
using System;
using CookingLah.F_Services;
using System.Web.UI;

namespace CookingLah.Pages.Admin
{
    public partial class Admin : System.Web.UI.MasterPage
    {
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
            if (!DatabaseManager.TryLoadMasterProfile(Session["UserId"], out username, out profileImage)) return;

            Session["Username"] = username;
            lblProfileName.Text = username;
            imgLearnerProfile.ImageUrl = profileImage;
        }

        protected void Logout_Click(object sender, System.EventArgs e)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Response.Redirect(ResolveUrl("~/Main.aspx"));
        }
    }
}
