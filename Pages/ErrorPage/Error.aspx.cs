using System;
using System.Web;

namespace CookingLah.Pages.ErrorPage
{
    public partial class ErrorPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int statusCode = GetStatusCode();
            ErrorDetails details = GetErrorDetails(statusCode);

            Response.StatusCode = statusCode;
            Response.SuppressFormsAuthenticationRedirect = true;
            Response.TrySkipIisCustomErrors = true;
            Response.Cache.SetNoStore();

            litPageTitle.Text = Server.HtmlEncode(details.PageTitle);
            litStatusCode.Text = statusCode.ToString();
            litHeading.Text = Server.HtmlEncode(details.Heading);
            litMessage.Text = Server.HtmlEncode(details.Message);
            litIllustration.Text = details.Illustration;
            lnkReload.NavigateUrl = GetSafeReturnUrl();
        }

        private int GetStatusCode()
        {
            int statusCode;
            if (!int.TryParse(Request.QueryString["code"], out statusCode))
            {
                return 500;
            }

            switch (statusCode)
            {
                case 400:
                case 401:
                case 403:
                case 404:
                case 408:
                case 429:
                case 500:
                case 503:
                    return statusCode;
                default:
                    return 500;
            }
        }

        private string GetSafeReturnUrl()
        {
            string returnUrl = Request.QueryString["returnUrl"] ?? "";
            if (returnUrl.StartsWith("/", StringComparison.Ordinal) &&
                !returnUrl.StartsWith("//", StringComparison.Ordinal) &&
                !returnUrl.StartsWith("/Pages/ErrorPage/", StringComparison.OrdinalIgnoreCase))
            {
                return returnUrl;
            }

            return ResolveUrl("~/Main.aspx");
        }

        private static ErrorDetails GetErrorDetails(int statusCode)
        {
            if (statusCode == 404)
            {
                return new ErrorDetails(
                    "Page not found",
                    "This recipe page is missing",
                    "The page may have moved, or the address may be incorrect.",
                    MissingPageIllustration());
            }

            if (statusCode == 401 || statusCode == 403)
            {
                return new ErrorDetails(
                    "Access unavailable",
                    "This kitchen is not open to you",
                    "Sign in with the correct account or return to the main page.",
                    AccessIllustration());
            }

            if (statusCode == 400 || statusCode == 408 || statusCode == 429)
            {
                return new ErrorDetails(
                    "Request interrupted",
                    "We could not complete that request",
                    "Check your connection, wait a moment, and try the page again.",
                    RequestIllustration());
            }

            if (statusCode == 503)
            {
                return new ErrorDetails(
                    "Service unavailable",
                    "The kitchen is temporarily closed",
                    "Cooking LAH is unavailable right now. Please try again shortly.",
                    ServerIllustration());
            }

            return new ErrorDetails(
                "Something went wrong",
                "Something spilled in the kitchen",
                "The problem has been recorded. Reload the page or return to the main page.",
                ServerIllustration());
        }

        private static string MissingPageIllustration()
        {
            return @"<svg class='error-svg' viewBox='0 0 260 210' role='img' aria-label='An empty serving plate with a question mark'><path d='M37 151h186c-8 28-35 42-93 42s-85-14-93-42Z' fill='#FFF9EB' stroke='#35030C' stroke-width='7'/><path d='M56 151c6-48 31-76 74-76s68 28 74 76' fill='#FFD24C' stroke='#35030C' stroke-width='7'/><path d='M130 74V52' stroke='#35030C' stroke-width='7' stroke-linecap='round'/><circle cx='130' cy='42' r='10' fill='#F93209' stroke='#35030C' stroke-width='6'/><path d='M113 110c2-16 33-19 35 0 1 11-15 12-15 24' fill='none' stroke='#35030C' stroke-width='7' stroke-linecap='round'/><circle cx='133' cy='143' r='4' fill='#35030C'/></svg>";
        }

        private static string AccessIllustration()
        {
            return @"<svg class='error-svg' viewBox='0 0 260 210' role='img' aria-label='A locked cooking pot'><path d='M48 88h164l-13 92H61L48 88Z' fill='#B99CFF' stroke='#35030C' stroke-width='7'/><path d='M35 88h190M96 68h68' stroke='#35030C' stroke-width='7' stroke-linecap='round'/><rect x='102' y='115' width='56' height='49' rx='8' fill='#FFF9EB' stroke='#35030C' stroke-width='6'/><path d='M115 115V99a15 15 0 0 1 30 0v16' fill='none' stroke='#35030C' stroke-width='6'/><circle cx='130' cy='139' r='6' fill='#F93209'/></svg>";
        }

        private static string RequestIllustration()
        {
            return @"<svg class='error-svg' viewBox='0 0 260 210' role='img' aria-label='A kitchen timer asking the user to try again'><circle cx='130' cy='118' r='70' fill='#CDF58A' stroke='#35030C' stroke-width='7'/><path d='M130 118V73M130 118l34 22M105 30h50M130 48V30' stroke='#35030C' stroke-width='7' stroke-linecap='round'/><path d='M61 69 44 52M199 69l17-17' stroke='#F93209' stroke-width='8' stroke-linecap='round'/><circle cx='130' cy='118' r='7' fill='#F93209'/></svg>";
        }

        private static string ServerIllustration()
        {
            return @"<svg class='error-svg' viewBox='0 0 260 210' role='img' aria-label='A cooking pot with a warning symbol'><path d='M45 86h170l-16 95H61L45 86Z' fill='#FBB8EE' stroke='#35030C' stroke-width='7'/><path d='M32 86h196M93 67h74' stroke='#35030C' stroke-width='7' stroke-linecap='round'/><path d='m130 105 29 52h-58l29-52Z' fill='#FFD24C' stroke='#35030C' stroke-width='6'/><path d='M130 123v16' stroke='#35030C' stroke-width='6' stroke-linecap='round'/><circle cx='130' cy='149' r='4' fill='#35030C'/><path d='M83 42c-9-12 9-16 0-29M130 42c-9-12 9-16 0-29M177 42c-9-12 9-16 0-29' fill='none' stroke='#F93209' stroke-width='6' stroke-linecap='round'/></svg>";
        }

        private sealed class ErrorDetails
        {
            public ErrorDetails(string pageTitle, string heading, string message, string illustration)
            {
                PageTitle = pageTitle;
                Heading = heading;
                Message = message;
                Illustration = illustration;
            }

            public string PageTitle { get; private set; }
            public string Heading { get; private set; }
            public string Message { get; private set; }
            public string Illustration { get; private set; }
        }
    }
}
