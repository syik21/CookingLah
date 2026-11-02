using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CookingLah.Pages.Chef.CreateCourse;

namespace CookingLah.Pages.Shared.LessonBlocks
{
    public partial class LessonBlocks : UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Page.Header != null && Page.Header.FindControl("LessonBlocksStyles") == null)
            {
                Page.Header.Controls.Add(new HtmlLink
                {
                    ID = "LessonBlocksStyles",
                    Href = ResolveUrl("~/Pages/Shared/LessonBlocks/LessonBlocks.css"),
                    Attributes = { ["rel"] = "stylesheet" }
                });
            }
        }

        // The parent supplies content only after checking course access.
        public void BindBlocks(DataTable blocks, int lessonId)
        {
            rptBlocks.DataSource = new DataView(blocks)
            {
                RowFilter = "LessonID=" + lessonId,
                Sort = "ContentOrder ASC,LessonContentID ASC"
            };
            rptBlocks.DataBind();
        }

        protected string Media(object value)
        {
            return CourseBuilderData.MediaUrl(Convert.ToString(value));
        }
    }
}
