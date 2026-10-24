using System;
using System.Collections.Generic;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;

namespace CookingLah.Pages.Chef.CreateCourse.Activities
{
    public partial class ActivityForm : System.Web.UI.UserControl
    {
        public void LoadActivity(int courseId)
        {
            txtActivityTitle.Text = "";
            txtActivityInstructions.Text = "";
            var rows = Read("SELECT TOP 1 Title, Instructions FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID", Parameter("@id", courseId));
            if (rows.Rows.Count == 0) return;
            txtActivityTitle.Text = Convert.ToString(rows.Rows[0]["Title"]);
            txtActivityInstructions.Text = Convert.ToString(rows.Rows[0]["Instructions"]);
        }

        public List<string> ValidateActivity()
        {
            var errors = new List<string>();
            bool exists = !string.IsNullOrWhiteSpace(txtActivityTitle.Text + txtActivityInstructions.Text);
            if (exists && string.IsNullOrWhiteSpace(txtActivityTitle.Text)) errors.Add("Please enter the activity title, or clear the instructions to skip it.");
            if (exists && string.IsNullOrWhiteSpace(txtActivityInstructions.Text)) errors.Add("Please enter instructions for the practical activity.");
            if (txtActivityTitle.Text.Length > 150) errors.Add("Keep the activity title within 150 characters.");
            return errors;
        }

        public void SaveActivity(int courseId)
        {
            EditDraft(courseId, (connection, transaction) => {
                Execute(connection, transaction, "DELETE FROM PracticalActivities WHERE CourseID=@id", Parameter("@id", courseId));
                if (string.IsNullOrWhiteSpace(txtActivityTitle.Text + txtActivityInstructions.Text)) return;
                Execute(connection, transaction,
                    "INSERT INTO PracticalActivities (CourseID,Title,Instructions,CreatedAt) VALUES (@id,@title,@instructions,GETDATE())",
                    Parameter("@id", courseId), Parameter("@title", txtActivityTitle.Text.Trim()),
                    Parameter("@instructions", txtActivityInstructions.Text.Trim()));
            });
        }
    }
}
