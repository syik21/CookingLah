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

namespace CookingLah.Pages.Admin.UserReport
{
    public partial class UserReportPage : Page
    {
        private const int PageSize = 10;
        private bool isAuthorized;

        private int CurrentPage
        {
            get { return (int)(ViewState["CurrentPage"] ?? 1); }
            set { ViewState["CurrentPage"] = value; }
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

            isAuthorized = IsActiveAdmin();
            if (!isAuthorized)
            {
                return;
            }

            if (!IsPostBack)
            {
                LoadInitialReport();
            }
        }

        // Verify the current session against the active Admin record.
        private bool IsActiveAdmin()
        {
            int userId;
            if (!int.TryParse(Convert.ToString(Session["UserId"]), out userId))
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
                      AND Role='Admin'
                      AND AccountStatus='Active'", connection))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
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
                System.Diagnostics.Trace.TraceError("User report authorization: {0}", exception);
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

        // Load filter values and the complete report on first visit.
        private void LoadInitialReport()
        {
            try
            {
                using (SqlConnection connection = DatabaseManager.ConnectDB())
                {
                    connection.Open();
                    LoadFilters(connection);
                    ddlDateRange.SelectedValue = "30";
                    CurrentPage = 1;
                    LoadUserReport(connection);
                }
            }
            catch (Exception exception)
            {
                HandleReportError(exception);
            }
        }

        // Populate role and status filters from current database values.
        private void LoadFilters(SqlConnection connection)
        {
            LoadYearFilter(connection);
            LoadFilter(connection, ddlRole, "SELECT DISTINCT Role FROM dbo.Users WHERE Role IS NOT NULL AND Role<>'' ORDER BY Role", "All Roles");
            LoadFilter(connection, ddlStatus, "SELECT DISTINCT AccountStatus FROM dbo.Users WHERE AccountStatus IS NOT NULL AND AccountStatus<>'' ORDER BY AccountStatus", "All Status");
        }

        private void LoadYearFilter(SqlConnection connection)
        {
            const string query = @"
                SELECT ReportYear
                FROM
                (
                    SELECT DISTINCT YEAR(CreatedAt) AS ReportYear FROM dbo.Users
                    UNION
                    SELECT @currentYear
                ) years
                ORDER BY ReportYear DESC";

            ddlYear.Items.Clear();
            ddlYear.Items.Add(new ListItem("All Years", ""));
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@currentYear", SqlDbType.Int).Value = DateTime.Today.Year;
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

        private static void LoadFilter(SqlConnection connection, DropDownList list, string query, string allLabel)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                list.Items.Clear();
                list.Items.Add(new ListItem(allLabel, ""));

                while (reader.Read())
                {
                    string value = Convert.ToString(reader[0]);
                    list.Items.Add(new ListItem(value, value));
                }
            }
        }

        // Load all report sections with the current filters.
        private void LoadUserReport(SqlConnection connection)
        {
            ReportRange range = GetSelectedDateRange();
            litPeriodLabel.Text = range.Label;
            litNewUsersPeriod.Text = range.Label;
            LoadSummaryStatistics(connection, range);
            LoadRegistrationTrend(connection, range);
            LoadRoleDistribution(connection);
            LoadLoginActivity(connection);
            LoadInstructorActivity(connection);
            LoadUserActivity(connection, range);
        }

        private void ReloadUserReport()
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
                    LoadUserReport(connection);
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
                StartDate = today.AddDays(-29),
                EndDate = today.AddDays(1),
                Label = "Last 30 Days",
                GroupByDay = true
            };

            if (ddlDateRange.SelectedValue == "custom")
            {
                int year;
                bool hasYear = int.TryParse(ddlYear.SelectedValue, out year);
                int month;
                int.TryParse(ddlMonth.SelectedValue, out month);

                if (!hasYear && month == 0)
                {
                    range.StartDate = null;
                    range.Label = "All Time";
                    range.GroupByDay = false;
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
                    range.GroupByDay = false;
                }

                return range;
            }

            switch (ddlDateRange.SelectedValue)
            {
                case "7":
                    range.StartDate = today.AddDays(-6);
                    range.Label = "Last 7 Days";
                    break;
                case "3m":
                    range.StartDate = today.AddMonths(-3);
                    range.Label = "Last 3 Months";
                    range.GroupByDay = false;
                    break;
                case "6m":
                    range.StartDate = today.AddMonths(-6);
                    range.Label = "Last 6 Months";
                    range.GroupByDay = false;
                    break;
                case "year":
                    range.StartDate = new DateTime(today.Year, 1, 1);
                    range.Label = "This Year";
                    range.GroupByDay = false;
                    break;
                case "all":
                    range.StartDate = null;
                    range.Label = "All Time";
                    range.GroupByDay = false;
                    break;
            }

            return range;
        }

        // Load summary statistics for the selected user filters.
        private void LoadSummaryStatistics(SqlConnection connection, ReportRange range)
        {
            const string query = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Users u
                     WHERE (@role='' OR u.Role=@role) AND (@status='' OR u.AccountStatus=@status)) AS TotalUsers,
                    (SELECT COUNT(*) FROM dbo.Users u
                     WHERE (@role='' OR u.Role=@role) AND (@status='' OR u.AccountStatus=@status)
                       AND (@startDate IS NULL OR u.CreatedAt>=@startDate) AND u.CreatedAt<@endDate) AS NewUsers,
                    (SELECT COUNT(*) FROM dbo.Users u
                     WHERE (@role='' OR u.Role=@role) AND (@status='' OR u.AccountStatus=@status)
                       AND EXISTS (SELECT 1 FROM dbo.UserLoginLog l WHERE l.UserID=u.UserID
                           AND (@startDate IS NULL OR l.LoginDate>=@startDate) AND l.LoginDate<@endDate)) AS ActiveUsers";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddUserFilters(command);
                AddDateRange(command, range);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    int totalUsers = Convert.ToInt32(reader["TotalUsers"]);
                    int activeUsers = Convert.ToInt32(reader["ActiveUsers"]);
                    litTotalUsers.Text = totalUsers.ToString("N0");
                    litNewUsers.Text = Convert.ToInt32(reader["NewUsers"]).ToString("N0");
                    litActiveUsers.Text = activeUsers.ToString("N0");
                    litInactiveUsers.Text = Math.Max(0, totalUsers - activeUsers).ToString("N0");
                }
            }
        }

        // Load user registration data for the trend chart.
        private void LoadRegistrationTrend(SqlConnection connection, ReportRange range)
        {
            const string query = @"
                SELECT CAST(u.CreatedAt AS date) AS RegistrationDate, COUNT(*) AS UserCount
                FROM dbo.Users u
                WHERE (@role='' OR u.Role=@role)
                  AND (@status='' OR u.AccountStatus=@status)
                  AND (@startDate IS NULL OR u.CreatedAt>=@startDate)
                  AND u.CreatedAt<@endDate
                GROUP BY CAST(u.CreatedAt AS date)
                ORDER BY RegistrationDate";

            Dictionary<DateTime, int> dailyCounts = new Dictionary<DateTime, int>();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddUserFilters(command);
                AddDateRange(command, range);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dailyCounts[Convert.ToDateTime(reader["RegistrationDate"]).Date] = Convert.ToInt32(reader["UserCount"]);
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

            DateTime lastMonth = new DateTime(range.EndDate.AddDays(-1).Year, range.EndDate.AddDays(-1).Month, 1);
            List<ChartPoint> points = new List<ChartPoint>();
            for (DateTime month = firstMonth; month <= lastMonth; month = month.AddMonths(1))
            {
                int count;
                monthlyCounts.TryGetValue(month, out count);
                points.Add(new ChartPoint { label = month.ToString("MMM yyyy", CultureInfo.InvariantCulture), count = count });
            }

            return points;
        }

        // Load role totals for the user distribution bars.
        private void LoadRoleDistribution(SqlConnection connection)
        {
            const string query = @"
                SELECT u.Role, COUNT(*) AS UserCount
                FROM dbo.Users u
                WHERE (@role='' OR u.Role=@role)
                  AND (@status='' OR u.AccountStatus=@status)
                GROUP BY u.Role";

            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddUserFilters(command);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        counts[Convert.ToString(reader["Role"])] = Convert.ToInt32(reader["UserCount"]);
                    }
                }
            }

            string[] roles = { "Learner", "Instructor", "Admin" };
            int maximum = 0;
            foreach (string role in roles)
            {
                int count;
                counts.TryGetValue(role, out count);
                maximum = Math.Max(maximum, count);
            }

            DataTable distribution = new DataTable();
            distribution.Columns.Add("Role", typeof(string));
            distribution.Columns.Add("UserCount", typeof(int));
            distribution.Columns.Add("BarPercent", typeof(int));
            foreach (string role in roles)
            {
                int count;
                counts.TryGetValue(role, out count);
                int percentage = maximum == 0 ? 0 : (int)Math.Round(count * 100d / maximum);
                distribution.Rows.Add(role, count, percentage);
            }

            rptRoleDistribution.DataSource = distribution;
            rptRoleDistribution.DataBind();
        }

        // Load active user counts from the login log.
        private void LoadLoginActivity(SqlConnection connection)
        {
            const string query = @"
                SELECT
                    COUNT(DISTINCT CASE WHEN l.LoginDate=@today THEN l.UserID END) AS ActiveToday,
                    COUNT(DISTINCT CASE WHEN l.LoginDate>=@sevenDays AND l.LoginDate<@tomorrow THEN l.UserID END) AS ActiveSevenDays,
                    COUNT(DISTINCT CASE WHEN l.LoginDate>=@thirtyDays AND l.LoginDate<@tomorrow THEN l.UserID END) AS ActiveThirtyDays
                FROM dbo.UserLoginLog l
                INNER JOIN dbo.Users u ON u.UserID=l.UserID
                WHERE (@role='' OR u.Role=@role)
                  AND (@status='' OR u.AccountStatus=@status)";

            DateTime today = DateTime.Today;
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddUserFilters(command);
                command.Parameters.Add("@today", SqlDbType.Date).Value = today;
                command.Parameters.Add("@sevenDays", SqlDbType.Date).Value = today.AddDays(-6);
                command.Parameters.Add("@thirtyDays", SqlDbType.Date).Value = today.AddDays(-29);
                command.Parameters.Add("@tomorrow", SqlDbType.Date).Value = today.AddDays(1);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    litActiveToday.Text = Convert.ToInt32(reader["ActiveToday"]).ToString("N0");
                    litActiveSevenDays.Text = Convert.ToInt32(reader["ActiveSevenDays"]).ToString("N0");
                    litActiveThirtyDays.Text = Convert.ToInt32(reader["ActiveThirtyDays"]).ToString("N0");
                }
            }
        }

        // Load the top five instructors by courses created.
        private void LoadInstructorActivity(SqlConnection connection)
        {
            const string query = @"
                SELECT TOP (5) u.Fullname, u.Username, COUNT(c.CourseID) AS CourseCount
                FROM dbo.Users u
                LEFT JOIN dbo.Courses c ON c.InstructorID=u.UserID
                WHERE u.Role='Instructor'
                  AND (@role='' OR u.Role=@role)
                  AND (@status='' OR u.AccountStatus=@status)
                GROUP BY u.UserID, u.Fullname, u.Username
                HAVING COUNT(c.CourseID)>0
                ORDER BY CourseCount DESC, u.Fullname, u.UserID";

            DataTable instructors = new DataTable();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddUserFilters(command);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    instructors.Load(reader);
                }
            }

            rptInstructorActivity.DataSource = instructors;
            rptInstructorActivity.DataBind();
            pnlInstructorList.Visible = instructors.Rows.Count > 0;
            pnlNoInstructorActivity.Visible = instructors.Rows.Count == 0;
        }

        // Load user activity for the detailed report table.
        private void LoadUserActivity(SqlConnection connection, ReportRange range)
        {
            string search = txtSearch.Text.Trim();
            if (search.Length > 100)
            {
                search = search.Substring(0, 100);
                txtSearch.Text = search;
            }

            const string filters = @"
                WHERE (@role='' OR u.Role=@role)
                  AND (@status='' OR u.AccountStatus=@status)
                  AND (@search='' OR CHARINDEX(@search,u.Username)>0 OR CHARINDEX(@search,u.Email)>0 OR CHARINDEX(@search,u.Fullname)>0)";

            int totalUsers;
            using (SqlCommand countCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.Users u " + filters, connection))
            {
                AddUserFilters(countCommand);
                countCommand.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = search;
                totalUsers = Convert.ToInt32(countCommand.ExecuteScalar());
            }

            int pageCount = Math.Max(1, (totalUsers + PageSize - 1) / PageSize);
            CurrentPage = Math.Min(Math.Max(1, CurrentPage), pageCount);

            const string query = @"
                SELECT u.UserID, u.Fullname, u.Username, u.Email, u.Role, u.CreatedAt, u.LastLogin, u.AccountStatus,
                       COUNT(l.LoginLogID) AS ActiveDays
                FROM dbo.Users u
                LEFT JOIN dbo.UserLoginLog l ON l.UserID=u.UserID
                    AND (@startDate IS NULL OR l.LoginDate>=@startDate)
                    AND l.LoginDate<@endDate " + filters + @"
                GROUP BY u.UserID, u.Fullname, u.Username, u.Email, u.Role, u.CreatedAt, u.LastLogin, u.AccountStatus
                ORDER BY u.CreatedAt DESC, u.UserID DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            DataTable users = new DataTable();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                AddUserFilters(command);
                AddDateRange(command, range);
                command.Parameters.Add("@search", SqlDbType.NVarChar, 100).Value = search;
                command.Parameters.Add("@offset", SqlDbType.Int).Value = (CurrentPage - 1) * PageSize;
                command.Parameters.Add("@pageSize", SqlDbType.Int).Value = PageSize;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    users.Load(reader);
                }
            }

            rptUserActivity.DataSource = users;
            rptUserActivity.DataBind();
            lblResultCount.Text = totalUsers == 1 ? "1 User" : totalUsers.ToString("N0") + " Users";
            pnlUserTable.Visible = totalUsers > 0;
            pnlEmpty.Visible = totalUsers == 0;
            pnlPagination.Visible = totalUsers > PageSize;
            lblPage.Text = "Page " + CurrentPage + " of " + pageCount;
            btnPrevious.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < pageCount;
        }

        private void AddUserFilters(SqlCommand command)
        {
            command.Parameters.Add("@role", SqlDbType.NVarChar, 20).Value = ddlRole.SelectedValue;
            command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;
        }

        private static void AddDateRange(SqlCommand command, ReportRange range)
        {
            command.Parameters.Add("@startDate", SqlDbType.Date).Value = range.StartDate.HasValue
                ? (object)range.StartDate.Value.Date
                : DBNull.Value;
            command.Parameters.Add("@endDate", SqlDbType.Date).Value = range.EndDate.Date;
        }

        protected string FormatJoinedDate(object value)
        {
            return Convert.ToDateTime(value).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        protected string FormatLastLogin(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "Never";
            }

            return Convert.ToDateTime(value).ToString("dd MMM yyyy, h:mm tt", CultureInfo.InvariantCulture);
        }

        protected string GetStatusCss(object value)
        {
            switch (Convert.ToString(value))
            {
                case "Active":
                    return "ur-status ur-status-active";
                case "Inactive":
                    return "ur-status ur-status-inactive";
                default:
                    return "ur-status ur-status-suspended";
            }
        }

        private void HandleReportError(Exception exception)
        {
            System.Diagnostics.Trace.TraceError("User report: {0}", exception);
            ShowError();
        }

        private void ShowError()
        {
            litError.Text = "Unable to load the user report. Please try again.";
            pnlError.Visible = true;
            pnlReport.Visible = false;
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ReloadUserReport();
        }

        protected void DateRange_Changed(object sender, EventArgs e)
        {
            if (ddlDateRange.SelectedValue != "custom")
            {
                ddlYear.SelectedIndex = 0;
                ddlMonth.SelectedIndex = 0;
            }

            CurrentPage = 1;
            ReloadUserReport();
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
            ReloadUserReport();
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ReloadUserReport();
        }

        protected void ClearFilters_Click(object sender, EventArgs e)
        {
            ddlDateRange.SelectedValue = "30";
            ddlYear.SelectedIndex = 0;
            ddlMonth.SelectedIndex = 0;
            ddlRole.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            txtSearch.Text = "";
            CurrentPage = 1;
            ReloadUserReport();
        }

        protected void Previous_Click(object sender, EventArgs e)
        {
            CurrentPage = Math.Max(1, CurrentPage - 1);
            ReloadUserReport();
        }

        protected void Next_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            ReloadUserReport();
        }
    }
}
