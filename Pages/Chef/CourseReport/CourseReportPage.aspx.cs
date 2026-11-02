using CookingLah.F_Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Chef.CourseReport
{
    public partial class CourseReportPage : Page
    {
        private const int PageSize = 10;
        private const string CourseFilters = @"
            (@categoryId=0 OR c.CategoryID=@categoryId)
            AND (@status='' OR c.Status=@status)
            AND c.InstructorID=@instructorId
            AND (@startDate IS NULL OR (c.CreatedAt>=@startDate AND c.CreatedAt<@endDate))";
        private const string ProgressDateFilter = "(@startDate IS NULL OR (StartedAt>=@startDate AND StartedAt<@endDate))";
        private const string ReviewDateFilter = "(@startDate IS NULL OR (CreatedAt>=@startDate AND CreatedAt<@endDate))";
        private bool isAuthorized;
        private int instructorId;

        private int CurrentPage
        {
            get
            {
                return (int)(ViewState["CurrentPage"] ?? 1);
            }
            set
            {
                ViewState["CurrentPage"] = value;
            }
        }

        private sealed class ReportRange
        {
            public DateTime? StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Label { get; set; }
            public bool GroupByDay { get; set; }
        }

        private sealed class ChartPoint
        {
            public string label { get; set; }
            public int count { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            isAuthorized = IsActiveChef();
            if (!isAuthorized)
            {
                return;
            }

            if (!IsPostBack)
            {
                LoadInitialReport();
            }
        }

        // Verify the current session against the active chef record.
        private bool IsActiveChef()
        {
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out instructorId))
            {
                RedirectToHome();
                return false;
            }

            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                using (SqlCommand command = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM dbo.Users
                    WHERE UserID=@userId
                      AND Role='Instructor'
                      AND AccountStatus='Active'", connection))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = instructorId;
                    connection.Open();
                    if (Convert.ToInt32(command.ExecuteScalar()) == 1)
                    {
                        return true;
                    }
                }

                RedirectToHome();
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("Course report authorization: {0}", exception);
                ShowError();
            }

            return false;
        }

        private void RedirectToHome()
        {
            Response.Redirect(ResolveUrl("~/Main.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
            pnlReport.Visible = false;
        }

        // Load filters and all report sections on first visit.
        private void LoadInitialReport()
        {
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    LoadFilters(connection);
                    ddlDateRange.SelectedValue = "all";
                    CurrentPage = 1;
                    LoadCourseReport(connection);
                }
            }
            catch (Exception exception)
            {
                HandleReportError(exception);
            }
        }

        // Populate filters from this chef's courses.
        private void LoadFilters(SqlConnection connection)
        {
            LoadYearFilter(connection);
            LoadLookup(connection, ddlCategory, @"
                SELECT DISTINCT cat.CategoryID, cat.CategoryName
                FROM dbo.Categories cat
                INNER JOIN dbo.Courses c ON c.CategoryID=cat.CategoryID
                WHERE c.InstructorID=@instructorId
                ORDER BY cat.CategoryName, cat.CategoryID", "CategoryID", "CategoryName", "All Categories");
        }

        private void LoadYearFilter(SqlConnection connection)
        {
            const string query = @"
                SELECT ReportYear
                FROM
                (
                    SELECT DISTINCT YEAR(CreatedAt) AS ReportYear FROM dbo.Courses WHERE InstructorID=@instructorId
                    UNION
                    SELECT @currentYear
                ) years
                ORDER BY ReportYear DESC";

            ddlYear.Items.Clear();
            ddlYear.Items.Add(new ListItem("All Years", ""));
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@currentYear", SqlDbType.Int).Value = DateTime.Today.Year;
                command.Parameters.Add("@instructorId", SqlDbType.Int).Value = instructorId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string year = Convert.ToString(reader["ReportYear"]);
                        ddlYear.Items.Add(new ListItem(year, year));
                    }
                }
            }
        }

        private void LoadLookup(SqlConnection connection, DropDownList list, string query, string valueField, string textField, string allLabel)
        {
            DataTable values = new DataTable();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@instructorId", SqlDbType.Int).Value = instructorId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    values.Load(reader);
                }
            }

            list.DataSource = values;
            list.DataValueField = valueField;
            list.DataTextField = textField;
            list.DataBind();
            list.Items.Insert(0, new ListItem(allLabel, "0"));
        }

        // Load every section of the course report.
        private void LoadCourseReport(SqlConnection connection)
        {
            ReportRange range = GetSelectedDateRange();
            litPeriodLabel.Text = range.Label;
            LoadSummaryStatistics(connection, range);
            LoadCourseCreationTrend(connection, range);
            LoadCourseStatus(connection, range);
            LoadEnrollmentStatistics(connection, range);
            LoadMostEnrolledCourses(connection, range);
            LoadTopRatedCourses(connection, range);
            LoadCoursePerformance(connection, range);
        }

        private void ReloadCourseReport()
        {
            if (!isAuthorized)
            {
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    LoadCourseReport(connection);
                }
            }
            catch (Exception exception)
            {
                HandleReportError(exception);
            }
        }

        private ReportRange GetSelectedDateRange()
        {
            DateTime today = DateTime.Today;
            ReportRange range = new ReportRange
            {
                StartDate = null,
                EndDate = today.AddDays(1),
                Label = "All Time",
                GroupByDay = false
            };

            if (ddlDateRange.SelectedValue == "custom")
            {
                int year;
                bool hasYear = int.TryParse(ddlYear.SelectedValue, out year);
                int month;
                int.TryParse(ddlMonth.SelectedValue, out month);

                if (!hasYear && month == 0)
                {
                    return range;
                }

                if (!hasYear)
                {
                    year = today.Year;
                }

                if (month >= 1 && month <= 12)
                {
                    range.StartDate = new DateTime(year, month, 1);
                    range.EndDate = range.StartDate.Value.AddMonths(1);
                    range.Label = range.StartDate.Value.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                    range.GroupByDay = true;
                }
                else
                {
                    range.StartDate = new DateTime(year, 1, 1);
                    range.EndDate = range.StartDate.Value.AddYears(1);
                    range.Label = year.ToString(CultureInfo.InvariantCulture);
                }

                return range;
            }

            switch (ddlDateRange.SelectedValue)
            {
                case "30":
                    range.StartDate = today.AddDays(-29);
                    range.Label = "Last 30 Days";
                    range.GroupByDay = true;
                    break;
                case "3m":
                    range.StartDate = today.AddMonths(-3);
                    range.Label = "Last 3 Months";
                    break;
                case "6m":
                    range.StartDate = today.AddMonths(-6);
                    range.Label = "Last 6 Months";
                    break;
                case "year":
                    range.StartDate = new DateTime(today.Year, 1, 1);
                    range.Label = "This Year";
                    break;
            }

            return range;
        }

        // Load summary statistics for the current report filters.
        private void LoadSummaryStatistics(SqlConnection connection, ReportRange range)
        {
            string query = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Courses c WHERE " + CourseFilters + @") AS TotalCourses,
                    (SELECT COUNT(*) FROM dbo.Courses c WHERE " + CourseFilters + @" AND c.Status='Published') AS PublishedCourses,
                    (SELECT COUNT(cp.ProgressID)
                     FROM dbo.Courses c
                     INNER JOIN dbo.CourseProgress cp ON cp.CourseID=c.CourseID
                     WHERE " + CourseFilters + " AND " + ProgressDateFilter.Replace("StartedAt", "cp.StartedAt") + @") AS TotalEnrolments,
                    (SELECT COUNT(r.ReviewID)
                     FROM dbo.Courses c
                     INNER JOIN dbo.Reviews r ON r.CourseID=c.CourseID
                     WHERE " + CourseFilters + " AND " + ReviewDateFilter.Replace("CreatedAt", "r.CreatedAt") + @") AS ReviewCount,
                    (SELECT AVG(CAST(r.Rating AS DECIMAL(5,2)))
                     FROM dbo.Courses c
                     INNER JOIN dbo.Reviews r ON r.CourseID=c.CourseID
                     WHERE " + CourseFilters + " AND " + ReviewDateFilter.Replace("CreatedAt", "r.CreatedAt") + @") AS AverageRating";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddReportParameters(command, range);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    int reviewCount = Convert.ToInt32(reader["ReviewCount"]);
                    litTotalCourses.Text = Convert.ToInt32(reader["TotalCourses"]).ToString("N0");
                    litPublishedCourses.Text = Convert.ToInt32(reader["PublishedCourses"]).ToString("N0");
                    litTotalEnrolments.Text = Convert.ToInt32(reader["TotalEnrolments"]).ToString("N0");
                    litAverageRating.Text = reviewCount == 0 ? "—" : "★ " + Convert.ToDecimal(reader["AverageRating"]).ToString("0.0") + " / 5";
                    litReviewSummary.Text = reviewCount == 0 ? "No reviews yet" : reviewCount.ToString("N0") + (reviewCount == 1 ? " Review" : " Reviews");
                }
            }
        }

        // Load course creation data for the trend chart.
        private void LoadCourseCreationTrend(SqlConnection connection, ReportRange range)
        {
            string query = @"
                SELECT CAST(c.CreatedAt AS date) AS CreationDate, COUNT(*) AS CourseCount
                FROM dbo.Courses c
                WHERE " + CourseFilters + @"
                GROUP BY CAST(c.CreatedAt AS date)
                ORDER BY CreationDate";

            Dictionary<DateTime, int> dailyCounts = new Dictionary<DateTime, int>();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddReportParameters(command, range);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dailyCounts[Convert.ToDateTime(reader["CreationDate"]).Date] = Convert.ToInt32(reader["CourseCount"]);
                    }
                }
            }

            List<ChartPoint> points = range.GroupByDay
                ? BuildDailyChartPoints(range, dailyCounts)
                : BuildMonthlyChartPoints(range, dailyCounts);
            hfTrendData.Value = new JavaScriptSerializer().Serialize(points);
        }

        private static List<ChartPoint> BuildDailyChartPoints(ReportRange range, Dictionary<DateTime, int> dailyCounts)
        {
            List<ChartPoint> points = new List<ChartPoint>();
            DateTime firstDate = range.StartDate ?? DateTime.Today;
            for (DateTime date = firstDate.Date; date < range.EndDate.Date; date = date.AddDays(1))
            {
                int count;
                dailyCounts.TryGetValue(date, out count);
                points.Add(new ChartPoint { label = date.ToString("d MMM", CultureInfo.InvariantCulture), count = count });
            }
            return points;
        }

        private static List<ChartPoint> BuildMonthlyChartPoints(ReportRange range, Dictionary<DateTime, int> dailyCounts)
        {
            Dictionary<DateTime, int> monthlyCounts = new Dictionary<DateTime, int>();
            foreach (KeyValuePair<DateTime, int> item in dailyCounts)
            {
                DateTime month = new DateTime(item.Key.Year, item.Key.Month, 1);
                monthlyCounts[month] = monthlyCounts.ContainsKey(month) ? monthlyCounts[month] + item.Value : item.Value;
            }

            DateTime firstMonth;
            if (range.StartDate.HasValue)
            {
                firstMonth = new DateTime(range.StartDate.Value.Year, range.StartDate.Value.Month, 1);
            }
            else if (dailyCounts.Count > 0)
            {
                DateTime earliest = DateTime.MaxValue;
                foreach (DateTime date in dailyCounts.Keys)
                {
                    if (date < earliest)
                    {
                        earliest = date;
                    }
                }
                firstMonth = new DateTime(earliest.Year, earliest.Month, 1);
            }
            else
            {
                firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            DateTime finalDate = range.EndDate.AddDays(-1);
            DateTime lastMonth = new DateTime(finalDate.Year, finalDate.Month, 1);
            List<ChartPoint> points = new List<ChartPoint>();
            for (DateTime month = firstMonth; month <= lastMonth; month = month.AddMonths(1))
            {
                int count;
                monthlyCounts.TryGetValue(month, out count);
                points.Add(new ChartPoint { label = month.ToString("MMM yyyy", CultureInfo.InvariantCulture), count = count });
            }
            return points;
        }

        // Load totals for each course workflow status.
        private void LoadCourseStatus(SqlConnection connection, ReportRange range)
        {
            string query = "SELECT c.Status,COUNT(*) AS CourseCount FROM dbo.Courses c WHERE " + CourseFilters + " GROUP BY c.Status";
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddReportParameters(command, range);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        counts[Convert.ToString(reader["Status"])] = Convert.ToInt32(reader["CourseCount"]);
                    }
                }
            }

            string[] statuses = { "Published", "Draft", "Pending", "Rejected" };
            int maximum = 0;
            foreach (string status in statuses)
            {
                int count;
                counts.TryGetValue(status, out count);
                maximum = Math.Max(maximum, count);
            }

            DataTable statusRows = new DataTable();
            statusRows.Columns.Add("Status", typeof(string));
            statusRows.Columns.Add("CourseCount", typeof(int));
            statusRows.Columns.Add("BarPercent", typeof(int));
            foreach (string status in statuses)
            {
                int count;
                counts.TryGetValue(status, out count);
                int percentage = maximum == 0 ? 0 : (int)Math.Round(count * 100d / maximum);
                statusRows.Rows.Add(status, count, percentage);
            }
            rptCourseStatus.DataSource = statusRows;
            rptCourseStatus.DataBind();
        }

        // Load enrolment and completion statistics.
        private void LoadEnrollmentStatistics(SqlConnection connection, ReportRange range)
        {
            string query = @"
                SELECT COUNT(cp.ProgressID) AS TotalEnrolments,
                       SUM(CASE WHEN cp.Status='Completed' THEN 1 ELSE 0 END) AS CompletedCount,
                       SUM(CASE WHEN cp.Status='In Progress' THEN 1 ELSE 0 END) AS InProgressCount,
                       SUM(CASE WHEN cp.Status='Submitted' THEN 1 ELSE 0 END) AS SubmittedCount
                FROM dbo.Courses c
                INNER JOIN dbo.CourseProgress cp ON cp.CourseID=c.CourseID
                WHERE " + CourseFilters + " AND " + ProgressDateFilter.Replace("StartedAt", "cp.StartedAt");

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddReportParameters(command, range);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    int total = Convert.ToInt32(reader["TotalEnrolments"]);
                    int completed = reader["CompletedCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CompletedCount"]);
                    int inProgress = reader["InProgressCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["InProgressCount"]);
                    int submitted = reader["SubmittedCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SubmittedCount"]);
                    litEnrolmentTotal.Text = total.ToString("N0");
                    litCompleted.Text = completed.ToString("N0");
                    litInProgress.Text = inProgress.ToString("N0");
                    litSubmitted.Text = submitted.ToString("N0");
                    litCompletionRate.Text = total == 0 ? "—" : (completed * 100d / total).ToString("0.0") + "%";
                }
            }
        }

        // Load the five courses with the most enrolments.
        private void LoadMostEnrolledCourses(SqlConnection connection, ReportRange range)
        {
            string query = @"
                SELECT TOP (5) c.Title,COUNT(cp.ProgressID) AS EnrolmentCount
                FROM dbo.Courses c
                INNER JOIN dbo.CourseProgress cp ON cp.CourseID=c.CourseID
                    AND " + ProgressDateFilter.Replace("StartedAt", "cp.StartedAt") + @"
                WHERE " + CourseFilters + @"
                GROUP BY c.CourseID,c.Title
                ORDER BY EnrolmentCount DESC,c.Title,c.CourseID";

            DataTable courses = ReadTable(connection, query, range);
            rptMostEnrolled.DataSource = courses;
            rptMostEnrolled.DataBind();
            pnlMostEnrolled.Visible = courses.Rows.Count > 0;
            pnlNoEnrolments.Visible = courses.Rows.Count == 0;
        }

        // Load the five highest-rated reviewed courses.
        private void LoadTopRatedCourses(SqlConnection connection, ReportRange range)
        {
            string query = @"
                SELECT TOP (5) c.Title,AVG(CAST(r.Rating AS DECIMAL(5,2))) AS AverageRating,COUNT(r.ReviewID) AS ReviewCount
                FROM dbo.Courses c
                INNER JOIN dbo.Reviews r ON r.CourseID=c.CourseID
                    AND " + ReviewDateFilter.Replace("CreatedAt", "r.CreatedAt") + @"
                WHERE " + CourseFilters + @"
                GROUP BY c.CourseID,c.Title
                ORDER BY AverageRating DESC,ReviewCount DESC,c.Title,c.CourseID";

            DataTable courses = ReadTable(connection, query, range);
            rptTopRated.DataSource = courses;
            rptTopRated.DataBind();
            pnlTopRated.Visible = courses.Rows.Count > 0;
            pnlNoRatings.Visible = courses.Rows.Count == 0;
        }

        private DataTable ReadTable(SqlConnection connection, string query, ReportRange range)
        {
            DataTable table = new DataTable();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddReportParameters(command, range);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }
            }
            return table;
        }

        // Load detailed performance data for each course.
        private void LoadCoursePerformance(SqlConnection connection, ReportRange range)
        {
            string search = txtSearch.Text.Trim();
            if (search.Length > 100)
            {
                search = search.Substring(0, 100);
                txtSearch.Text = search;
            }

            string searchFilter = " AND (@search='' OR CHARINDEX(@search,c.Title)>0)";
            int totalCourses;
            using (SqlCommand countCommand = new SqlCommand(@"
                SELECT COUNT(*)
                FROM dbo.Courses c
                WHERE " + CourseFilters + searchFilter, connection))
            {
                AddReportParameters(countCommand, range);
                countCommand.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = search;
                totalCourses = Convert.ToInt32(countCommand.ExecuteScalar());
            }

            int pageCount = Math.Max(1, (totalCourses + PageSize - 1) / PageSize);
            CurrentPage = Math.Min(Math.Max(1, CurrentPage), pageCount);
            string query = @"
                 SELECT c.CourseID,c.Title,c.Status,cat.CategoryName,
                       ISNULL(p.EnrolmentCount,0) AS EnrolmentCount,
                       ISNULL(p.CompletedCount,0) AS CompletedCount,
                       ISNULL(r.ReviewCount,0) AS ReviewCount,r.AverageRating
                FROM dbo.Courses c
                INNER JOIN dbo.Categories cat ON cat.CategoryID=c.CategoryID
                LEFT JOIN
                (
                    SELECT CourseID,COUNT(ProgressID) AS EnrolmentCount,
                           SUM(CASE WHEN Status='Completed' THEN 1 ELSE 0 END) AS CompletedCount
                    FROM dbo.CourseProgress
                    WHERE " + ProgressDateFilter + @"
                    GROUP BY CourseID
                ) p ON p.CourseID=c.CourseID
                LEFT JOIN
                (
                    SELECT CourseID,COUNT(ReviewID) AS ReviewCount,
                           AVG(CAST(Rating AS DECIMAL(5,2))) AS AverageRating
                    FROM dbo.Reviews
                    WHERE " + ReviewDateFilter + @"
                    GROUP BY CourseID
                ) r ON r.CourseID=c.CourseID
                WHERE " + CourseFilters + searchFilter + @"
                ORDER BY c.CreatedAt DESC,c.CourseID DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            DataTable courses = new DataTable();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddReportParameters(command, range);
                command.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = search;
                command.Parameters.Add("@offset", SqlDbType.Int).Value = (CurrentPage - 1) * PageSize;
                command.Parameters.Add("@pageSize", SqlDbType.Int).Value = PageSize;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    courses.Load(reader);
                }
            }

            rptCoursePerformance.DataSource = courses;
            rptCoursePerformance.DataBind();
            lblResultCount.Text = totalCourses == 1 ? "1 Course" : totalCourses.ToString("N0") + " Courses";
            pnlCourseTable.Visible = totalCourses > 0;
            pnlEmpty.Visible = totalCourses == 0;
            pnlPagination.Visible = totalCourses > PageSize;
            lblPage.Text = "Page " + CurrentPage + " of " + pageCount;
            btnPrevious.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < pageCount;
        }

        private void AddReportParameters(SqlCommand command, ReportRange range)
        {
            int categoryId;
            int.TryParse(ddlCategory.SelectedValue, out categoryId);
            command.Parameters.Add("@categoryId", SqlDbType.Int).Value = categoryId;
            command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;
            command.Parameters.Add("@instructorId", SqlDbType.Int).Value = instructorId;
            command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = range.StartDate.HasValue
                ? (object)range.StartDate.Value
                : DBNull.Value;
            command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = range.EndDate;
        }

        protected string FormatRating(object value)
        {
            return Convert.ToDecimal(value).ToString("0.0");
        }

        protected string FormatCourseRating(object reviewCountValue, object ratingValue)
        {
            return Convert.ToInt32(reviewCountValue) == 0 || ratingValue == DBNull.Value
                ? "—"
                : "★ " + Convert.ToDecimal(ratingValue).ToString("0.0");
        }

        protected string FormatCompletionRate(object enrolmentValue, object completedValue)
        {
            int enrolments = Convert.ToInt32(enrolmentValue);
            return enrolments == 0
                ? "—"
                : (Convert.ToInt32(completedValue) * 100d / enrolments).ToString("0.0") + "%";
        }

        protected string GetStatusCss(object value)
        {
            return "cr-status cr-status-" + Convert.ToString(value).ToLowerInvariant();
        }

        private void HandleReportError(Exception exception)
        {
            System.Diagnostics.Trace.TraceError("Course report: {0}", exception);
            ShowError();
        }

        private void ShowError()
        {
            litError.Text = "Unable to load the course report. Please try again.";
            pnlError.Visible = true;
            pnlReport.Visible = false;
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ReloadCourseReport();
        }

        protected void DateRange_Changed(object sender, EventArgs e)
        {
            if (ddlDateRange.SelectedValue != "custom")
            {
                ddlYear.SelectedIndex = 0;
                ddlMonth.SelectedIndex = 0;
            }

            CurrentPage = 1;
            ReloadCourseReport();
        }

        protected void CalendarFilter_Changed(object sender, EventArgs e)
        {
            ddlDateRange.SelectedValue = "custom";
            if (ddlMonth.SelectedIndex > 0 && ddlYear.SelectedIndex == 0)
            {
                ListItem currentYear = ddlYear.Items.FindByValue(DateTime.Today.Year.ToString(CultureInfo.InvariantCulture));
                if (currentYear != null)
                {
                    ddlYear.ClearSelection();
                    currentYear.Selected = true;
                }
            }

            CurrentPage = 1;
            ReloadCourseReport();
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ReloadCourseReport();
        }

        protected void ClearFilters_Click(object sender, EventArgs e)
        {
            ddlDateRange.SelectedValue = "all";
            ddlYear.SelectedIndex = 0;
            ddlMonth.SelectedIndex = 0;
            ddlCategory.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            txtSearch.Text = "";
            CurrentPage = 1;
            ReloadCourseReport();
        }

        protected void Previous_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(1, CurrentPage - 1);
            ReloadCourseReport();
        }

        protected void Next_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            ReloadCourseReport();
        }
    }
}
