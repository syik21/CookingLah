using System;
using System.Web.UI;

namespace CookingLah.Pages.Guest.SearchCourse
{
    public partial class SearchCoursePage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SearchCourse1.IsLearner = false;
        }
    }
}

