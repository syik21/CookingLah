using System;
using System.Data;
using System.Web.UI;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Learner.SearchCourse
{
    public partial class SearchCoursePage : Page
    {
        private bool IsAuthorized { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int learnerId;
            if (Convert.ToString(Session["Role"]) != "Learner" ||
                !int.TryParse(Convert.ToString(Session["UserId"]), out learnerId) || learnerId <= 0)
            {
                Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            DataTable learner = Read(@"SELECT UserID FROM Users
                WHERE UserID=@learnerId AND Role='Learner' AND AccountStatus='Active'",
                Parameter("@learnerId", learnerId));
            if (learner.Rows.Count == 0)
            {
                Response.Redirect(ResolveUrl("~/Pages/Shared/LoginSignupPage.aspx?tab=login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            IsAuthorized = true;
            SearchCourse1.IsLearner = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthorized)
                return;
            SearchCourse1.IsLearner = true;
            int courseId;
            if (!IsPostBack && int.TryParse(Request.QueryString["courseId"], out courseId) && courseId > 0)
                SearchCourse1.OpenCourse(courseId);
        }
    }
}

