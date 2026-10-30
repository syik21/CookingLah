using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using CookingLah.F_Services;
using System.Diagnostics;

namespace CookingLah
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_AcquireRequestState(object sender, EventArgs e)
        {
            AuthenticationManager.RestoreSession(Context);
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception == null || Request.AppRelativeCurrentExecutionFilePath.StartsWith(
                "~/Pages/ErrorPage/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            HttpException httpException = exception as HttpException;
            int statusCode = httpException == null ? 500 : httpException.GetHttpCode();
            statusCode = NormalizeStatusCode(statusCode);

            Trace.TraceError("Unhandled request error at {0}: {1}", Request.RawUrl, exception);
            string returnUrl = HttpUtility.UrlEncode(Request.RawUrl);

            Server.ClearError();
            Response.Clear();
            Response.Redirect(
                VirtualPathUtility.ToAbsolute("~/Pages/ErrorPage/Error.aspx") +
                    "?code=" + statusCode + "&returnUrl=" + returnUrl,
                false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static int NormalizeStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 400:
                case 401:
                case 403:
                case 404:
                case 408:
                case 429:
                case 503:
                    return statusCode;
                default:
                    return 500;
            }
        }
    }
}
