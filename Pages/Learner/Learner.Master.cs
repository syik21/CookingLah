using CookingLah.F_Helpers;
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
            lblProfileName.Text = Session["Username"].ToString();
        }

        private void ValidateLearnerSession()
        {
            // TODO: Verify authentication.
            // TODO: Verify that the logged-in role is Learner.
        }

        private void LoadLearnerProfile()
        {
            // TODO: Retrieve learner name and profile image from Users.
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
