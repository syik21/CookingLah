using System;
using System.Data;
using static CookingLah.Pages.Chef.CreateCourse.CourseBuilderData;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse.Activity
{
    public partial class CourseActivity : CourseManagementControl
    {
        private int RecordId { get { return (int?)ViewState["RecordId"] ?? 0; } set { ViewState["RecordId"] = value; } }
        private DataTable GetRows() { return ReadDraftSection(CourseId, "PracticalActivities", "SELECT TOP 1 * FROM PracticalActivities WHERE CourseID=@id ORDER BY ActivityID", Parameter("@id", CourseId)); }
        public void LoadActivity(int courseId)
        {
            var rows = GetRows();
            rptDetails.DataSource = rows;
            rptDetails.DataBind();
            pnlEmpty.Visible = rows.Rows.Count == 0;
            btnAdd.Visible = CanEdit;
            btnEdit.Visible = btnDelete.Visible = CanEdit && rows.Rows.Count > 0;
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (Visible) Run(() => LoadActivity(CourseId));
            base.OnPreRender(e);
        }
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var rows = GetRows();
                DataRow row = rows.Rows.Count == 0 ? null : rows.Rows[0];
                RecordId = row == null ? 0 : Convert.ToInt32(row["ActivityID"]);
                txtTitle.Text = row == null ? "" : Convert.ToString(row["Title"]);
                txtInstructions.Text = row == null ? "" : Convert.ToString(row["Instructions"]);
                litEditorTitle.Text = RecordId == 0 ? "Add Practical Activity" : "Edit Practical Activity";
                pnlEditor.Visible = true;
            });
        }
        protected void btnCancel_Click(object sender, EventArgs e) { pnlEditor.Visible = pnlDelete.Visible = false; }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!pnlEditor.Visible) return;
            Run(() =>
            {
                EnsureEditable();
                string title = Required(txtTitle.Text, "an activity title", 150);
                string instructions = Required(txtInstructions.Text, "activity instructions");
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    DataRow row = RecordId == 0 ? null : CourseReviewDraft.FindRow(draft.Activities, "ActivityID", RecordId);
                    if (row == null)
                    {
                        if (draft.Activities.Rows.Count > 0) throw new InvalidOperationException("This activity changed. Reload the page and try again.");
                        row = draft.Activities.NewRow(); row["ActivityID"] = CourseReviewDraft.NewId(draft.Activities, "ActivityID"); row["CourseID"] = CourseId;
                        draft.Activities.Rows.Add(row);
                    }
                    row["Title"] = title; row["Instructions"] = instructions;
                    Saved("Practical activity changes saved.");
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                {
                    if (RecordId == 0 && Execute(connection, transaction, "SELECT TOP 1 ActivityID FROM PracticalActivities WHERE CourseID=@course", Parameter("@course", CourseId)) != null)
                        throw new InvalidOperationException("This section has already been added. Reload the page to edit it.");
                    if (RecordId != 0 && Execute(connection, transaction, "SELECT ActivityID FROM PracticalActivities WHERE ActivityID=@id AND CourseID=@course", Parameter("@id", RecordId), Parameter("@course", CourseId)) == null)
                        throw new InvalidOperationException("This content no longer exists. Reload the page.");
                    string sql = RecordId == 0 ? "INSERT INTO PracticalActivities (CourseID,Title,Instructions) VALUES (@course,@title,@instructions)" : "UPDATE PracticalActivities SET Title=@title,Instructions=@instructions WHERE ActivityID=@id AND CourseID=@course";
                    Execute(connection, transaction, sql, Parameter("@course", CourseId), Parameter("@id", RecordId), Parameter("@title", title), Parameter("@instructions", instructions));
                });
                Saved("Practical Activity saved successfully.");
            });
        }
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            Run(() =>
            {
                EnsureEditable();
                var rows = GetRows();
                if (rows.Rows.Count == 0) throw new InvalidOperationException("This content no longer exists.");
                RecordId = Convert.ToInt32(rows.Rows[0]["ActivityID"]);
                litDeleteName.Text = Convert.ToString(rows.Rows[0]["Title"]);
                pnlDelete.Visible = true;
            });
        }
        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (!pnlDelete.Visible || RecordId == 0) return;
            Run(() =>
            {
                if (CanEdit)
                {
                    var draft = GetReviewDraft(CourseId, true);
                    var row = CourseReviewDraft.FindRow(draft.Activities, "ActivityID", RecordId);
                    if (row != null) draft.Activities.Rows.Remove(row);
                    Saved("Practical activity changes saved.");
                    return;
                }
                EditCourse(CourseId, (connection, transaction) =>
                    Execute(connection, transaction, "DELETE FROM PracticalActivities WHERE ActivityID=@id AND CourseID=@course", Parameter("@id", RecordId), Parameter("@course", CourseId)));
                Saved("Practical Activity deleted successfully.");
            });
        }
    }
}
