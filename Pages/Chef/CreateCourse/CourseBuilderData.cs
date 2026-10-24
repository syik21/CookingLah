using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Chef.CreateCourse
{
    // Shared connection, ownership and upload handling; feature SQL stays in its control.
    public static class CourseBuilderData
    {
        public static SqlParameter Parameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable Read(string sql, params SqlParameter[] parameters)
        {
            using (var connection = DatabaseManager.ConnectDB())
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.Parameters.AddRange(parameters);
                var rows = new DataTable();
                adapter.Fill(rows);
                return rows;
            }
        }

        public static object Execute(SqlConnection connection, SqlTransaction transaction,
            string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        public static void EditDraft(int courseId, Action<SqlConnection, SqlTransaction> save)
        {
            int instructorId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
            using (var connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var allowed = Execute(connection, transaction,
                        "SELECT c.CourseID FROM Courses c WITH (UPDLOCK, HOLDLOCK) " +
                        "JOIN Users u ON u.UserID=c.InstructorID WHERE c.CourseID=@id " +
                        "AND c.InstructorID=@user AND c.Status='Draft' " +
                        "AND u.Role='Instructor' AND u.AccountStatus='Active'",
                        Parameter("@id", courseId), Parameter("@user", instructorId));
                    if (allowed == null)
                        throw new InvalidOperationException("This course is no longer an editable draft.");
                    save(connection, transaction);
                    Execute(connection, transaction, "UPDATE Courses SET UpdatedAt=GETDATE() WHERE CourseID=@id",
                        Parameter("@id", courseId));
                    transaction.Commit();
                }
            }
        }

        public static string SaveUpload(FileUpload upload, string existingPath, bool video = false)
        {
            if (!upload.HasFile) return existingPath;
            int limit = video ? 20 * 1024 * 1024 : 3 * 1024 * 1024;
            if (upload.PostedFile.ContentLength > limit)
                throw new InvalidOperationException(video ? "Choose a video smaller than 20 MB." : "Choose an image smaller than 3 MB.");

            string folder = "~/F_Assets/CourseMedia/";
            string path = folder + Guid.NewGuid().ToString("N") + (video ? ".mp4" : ".jpg");
            string physicalFolder = HttpContext.Current.Server.MapPath(folder);
            Directory.CreateDirectory(physicalFolder);
            if (video)
            {
                byte[] header = new byte[12];
                upload.PostedFile.InputStream.Read(header, 0, header.Length);
                upload.PostedFile.InputStream.Position = 0;
                if (Path.GetExtension(upload.FileName).ToLowerInvariant() != ".mp4" ||
                    System.Text.Encoding.ASCII.GetString(header, 4, 4) != "ftyp")
                    throw new InvalidOperationException("Choose an MP4 video.");
                upload.SaveAs(HttpContext.Current.Server.MapPath(path));
                return path;
            }
            try
            {
                using (var image = System.Drawing.Image.FromStream(upload.PostedFile.InputStream))
                {
                    if ((long)image.Width * image.Height > 24000000)
                        throw new InvalidOperationException("Choose an image smaller than 24 megapixels.");
                    using (var bitmap = new Bitmap(image))
                        bitmap.Save(HttpContext.Current.Server.MapPath(path), ImageFormat.Jpeg);
                }
            }
            catch (ArgumentException) { throw new InvalidOperationException("Choose a valid JPG or PNG image."); }
            return path;
        }

        public static string MediaUrl(string path)
        {
            // Only generated local course media is rendered by the builder.
            return !string.IsNullOrEmpty(path) && path.StartsWith("~/F_Assets/CourseMedia/", StringComparison.Ordinal)
                ? VirtualPathUtility.ToAbsolute(path) : "";
        }
    }
}
