using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using static CookingLah.Pages.Chef.ManageCourse.CourseManagementData;

namespace CookingLah.Pages.Chef.ManageCourse
{
    public class CourseOrderHandler : IHttpHandler, IRequiresSessionState
    {
        public class OrderRequest
        {
            public int courseId { get; set; }
            public string kind { get; set; }
            public int parentId { get; set; }
            public string ids { get; set; }
            public string token { get; set; }
        }

        public bool IsReusable { get { return false; } }

        // Save a drag-and-drop order without running the Web Forms page lifecycle.
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetNoStore();
            var serializer = new JavaScriptSerializer();

            try
            {
                if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Invalid order request.");

                string json;
                using (var reader = new StreamReader(context.Request.InputStream))
                    json = reader.ReadToEnd();
                var request = serializer.Deserialize<OrderRequest>(json);
                if (request == null) throw new InvalidOperationException("Invalid order request.");

                string expectedToken = Convert.ToString(context.Session["ManageCourseReorderToken"]);
                if (string.IsNullOrEmpty(expectedToken) || !string.Equals(expectedToken, request.token, StringComparison.Ordinal))
                    throw new InvalidOperationException("Your session changed. Reload the course and try again.");
                if (request.courseId <= 0 || !new[] { "lessons", "content", "questions" }.Contains(request.kind))
                    throw new InvalidOperationException("Invalid order request.");

                var draft = GetReviewDraft(request.courseId, true);
                draft.Reorder(request.kind, request.parentId, request.ids ?? "");
                WriteJson(context, serializer, true, "Order saved to the working copy.");
            }
            catch (InvalidOperationException ex)
            {
                WriteJson(context, serializer, false, ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Course reorder: {0}", ex);
                WriteJson(context, serializer, false, "Unable to save the new order. Please try again.");
            }
        }

        private static void WriteJson(HttpContext context, JavaScriptSerializer serializer, bool success, string message)
        {
            context.Response.Write(serializer.Serialize(new { success = success, message = message }));
        }
    }
}
