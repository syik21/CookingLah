using CookingLah.F_Helpers;
using CookingLah.F_Services;
using System;
using System.Web.Security;
using System.Web.UI;

namespace CookingLah.Pages.Learner
{
    public partial class LearnerMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null)
            {
                Response.Redirect(NavigationHelper.GetUrl("Main"));
                return;
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

        protected void Logout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            Response.Redirect(NavigationHelper.GetUrl("Home"));
        }
    }
}
