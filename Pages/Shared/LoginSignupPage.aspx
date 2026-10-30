<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginSignupPage.aspx.cs" Inherits="CookingLah.Pages.Shared.LoginSignupPage" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover" />
    <title>Log in - Cooking Lah</title>
    <link rel="icon" type="image/x-icon" href="<%= ResolveUrl("~/favicon.ico?v=2") %>" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="" />
    <link href="https://fonts.googleapis.com/css2?family=Caveat+Brush&family=Archivo+Black&family=Space+Mono:wght@400;700&display=swap" rel="stylesheet" />
    <link href="~/F_CSS/Style.css" rel="stylesheet" />
    <link href="~/F_CSS/LoginSignupPage.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">

        <header>
            <a class="logo" id="loginLogo" href="<%= ResolveUrl("~/Main.aspx") %>" data-admin-login-url="<%= ResolveUrl("~/Pages/Admin/LoginPage/AdminLoginPage.aspx") %>" aria-label="Cooking Lah home">
                <img src="<%= ResolveUrl("~/F_Assets/CookingLahlogo.png") %>" alt="Cooking Lah" />
            </a>
            <a class="btn alt" href="<%= ResolveUrl("~/Main.aspx") %>">&larr; Back home</a>
        </header>

        <main>
            <section class="intro">
                <h1>Back in <mark>the kitchen, lah.</mark></h1>
                <p>Log in to pick up your lessons, quizzes and live classes right where you left off. New here? Join in a minute.</p>

                <div class="stickers" aria-hidden="true">
                    <div class="st a"><img src="<%= ResolveUrl("~/F_Assets/LoginFood1.png") %>" alt="" /></div>
                    <div class="st b"><img src="<%= ResolveUrl("~/F_Assets/LoginFood2.png") %>" alt="" /></div>
                    <div class="st c"><img src="<%= ResolveUrl("~/F_Assets/LoginFood3.png") %>" alt="" /></div>
                    <div class="st d"><img src="<%= ResolveUrl("~/F_Assets/LoginFood4.png") %>" alt="" /></div>
                    <div class="st e"><img src="<%= ResolveUrl("~/F_Assets/LoginFood5.png") %>" alt="" /></div>
                    <div class="st f"><img src="<%= ResolveUrl("~/F_Assets/LoginFood6.png") %>" alt="" /></div>
                    <div class="st g"><img src="<%= ResolveUrl("~/F_Assets/LoginFood7.png") %>" alt="" /></div>
                </div>
            </section>

            <section class="card" id="card">
                <asp:Panel ID="pnlTabs" runat="server" CssClass="tabs">
                    <asp:LinkButton ID="lnkTabLogin" runat="server" CssClass="chip on" CausesValidation="false" OnClick="lnkTabLogin_Click">Log in</asp:LinkButton>
                    <asp:LinkButton ID="lnkTabSignup" runat="server" CssClass="chip" CausesValidation="false" OnClick="lnkTabSignup_Click">Sign up</asp:LinkButton>
                </asp:Panel>

                <asp:MultiView ID="mv" runat="server" ActiveViewIndex="0">

                    <%-- LOG IN --%>
                    <asp:View ID="vLogin" runat="server">
                        <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin" CssClass="view on">
                            <h2>Welcome back</h2>
                            <p class="sub">Log in with your username or email and password.</p>

                            <asp:Label ID="lblLoginError" runat="server" CssClass="errbox" role="alert" Visible="false" />

                            <div class="f">
                                <label class="l" for="txtLoginEmail">Username or email</label>
                                <asp:TextBox ID="txtLoginEmail" runat="server" ClientIDMode="Static" placeholder="Your username or email" autocomplete="username" MaxLength="100" />
                            </div>

                            <div class="f">
                                <label class="l" for="txtLoginPass">Password</label>
                                <asp:TextBox ID="txtLoginPass" runat="server" ClientIDMode="Static" TextMode="Password" placeholder="Enter your password" autocomplete="current-password" />
                                <button type="button" class="eye" data-t="txtLoginPass">Show</button>
                            </div>

                            <div class="row">
                                <asp:CheckBox ID="chkRemember" runat="server" Text="Remember me" CssClass="ck" />
                                <asp:LinkButton ID="lnkForgot" runat="server" CssClass="lk" CausesValidation="false" OnClick="lnkForgot_Click">Forgot password?</asp:LinkButton>
                            </div>

                            <asp:Button ID="btnLogin" runat="server" Text="Log in" CssClass="btn full" OnClick="btnLogin_Click" data-loading-text="Logging..." />

                            <p class="alt-line">Just browsing? <a class="lk" href="<%= ResolveUrl("~/Main.aspx") %>">Continue as guest</a></p>
                        </asp:Panel>
                    </asp:View>

                    <%-- SIGN UP --%>
                    <asp:View ID="vSignup" runat="server">
                        <asp:Panel ID="pnlSignup" runat="server" DefaultButton="btnSignup" CssClass="view on">
                            <h2>Join the kitchen</h2>
                            <p class="sub">Create a free account. It takes under a minute.</p>

                            <asp:Label ID="lblSignupError" runat="server" CssClass="errbox" role="alert" Visible="false" />

                            <span class="lab">I want to join as</span>
                            <asp:RadioButtonList ID="rblSignupRole" runat="server" ClientIDMode="Static" CssClass="roles" RepeatLayout="Flow" RepeatDirection="Horizontal">
                                <asp:ListItem Value="Learner" Selected="True">Learner</asp:ListItem>
                                <asp:ListItem Value="Chef">Chef / Instructor</asp:ListItem>
                            </asp:RadioButtonList>

                            <div class="f">
                                <label class="l" for="txtSignFullName">Full name (required)</label>
                                <asp:TextBox ID="txtSignFullName" runat="server" ClientIDMode="Static" placeholder="Your full name" autocomplete="name" MaxLength="100" aria-required="true" />
                            </div>

                            <div class="f">
                                <label class="l" for="txtSignName">Username</label>
                                <asp:TextBox ID="txtSignName" runat="server" ClientIDMode="Static" placeholder="Your username" autocomplete="username" MaxLength="50" />
                            </div>

                            <div class="f">
                                <label class="l" for="txtSignEmail">Email</label>
                                <asp:TextBox ID="txtSignEmail" runat="server" ClientIDMode="Static" TextMode="Email" placeholder="you@email.com" autocomplete="email" MaxLength="100" />
                            </div>

                            <div class="f">
                                <label class="l" for="txtSignPass">Password</label>
                                <asp:TextBox ID="txtSignPass" runat="server" ClientIDMode="Static" TextMode="Password" placeholder="At least 8 characters" autocomplete="new-password" />
                                <button type="button" class="eye" data-t="txtSignPass">Show</button>
                            </div>

                            <div class="f country-field">
                                <label class="l" id="countryFieldLabel" for="ddlCountry">Country</label>
                                <asp:DropDownList ID="ddlCountry" runat="server" ClientIDMode="Static" CssClass="form-select" EnableViewState="false" />
                                <button type="button" id="countryPickerOpen" class="country-trigger" aria-haspopup="true" aria-controls="countryDialog" aria-labelledby="countryFieldLabel countrySelected" hidden>
                                    <span id="countrySelected">Select your country</span>
                                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m7 10 5 5 5-5" /></svg>
                                </button>
                            </div>

                            <div id="professionFields">
                                <div class="f">
                                    <label class="l" for="ddlProfession">Profession</label>
                                    <asp:DropDownList ID="ddlProfession" runat="server" ClientIDMode="Static" CssClass="form-select">
                                        <asp:ListItem Value="">Choose your profession</asp:ListItem>
                                        <asp:ListItem Value="Chef">Chef</asp:ListItem>
                                        <asp:ListItem Value="Experienced Cooker">Experienced Cooker</asp:ListItem>
                                        <asp:ListItem Value="Pastry Chef">Pastry Chef</asp:ListItem>
                                        <asp:ListItem Value="Culinary Instructor">Culinary Instructor</asp:ListItem>
                                        <asp:ListItem Value="Others">Others</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="f" id="otherProfessionField">
                                    <label class="l" for="txtOtherProfession">Your profession</label>
                                    <asp:TextBox ID="txtOtherProfession" runat="server" ClientIDMode="Static" placeholder="Enter your profession" MaxLength="100" />
                                </div>
                            </div>

                            <asp:Button ID="btnSignup" runat="server" Text="Create account" CssClass="btn full" OnClick="btnSignup_Click" data-loading-text="Signing up..." />

                            <p class="alt-line">Chef accounts are reviewed before publishing content.</p>
                        </asp:Panel>
                    </asp:View>

                    <%-- FORGOT PASSWORD --%>
                    <asp:View ID="vForgot" runat="server">
                        <asp:Panel ID="pnlForgot" runat="server" DefaultButton="btnForgot" CssClass="view on">
                            <h2>Forgot it?</h2>
                            <p class="sub">Enter your email and we'll send a link to reset your password.</p>

                            <asp:Label ID="lblForgotError" runat="server" CssClass="errbox" role="alert" Visible="false" />

                            <div class="f">
                                <label class="l" for="txtForgotEmail">Email</label>
                                <asp:TextBox ID="txtForgotEmail" runat="server" ClientIDMode="Static" TextMode="Email" placeholder="you@email.com" autocomplete="email" />
                            </div>

                            <asp:Button ID="btnForgot" runat="server" Text="Send password reset" CssClass="btn full" OnClick="btnForgot_Click" data-loading-text="Sending..." />

                            <p class="alt-line">
                                <asp:LinkButton ID="lnkBackToLogin" runat="server" CssClass="lk" CausesValidation="false" OnClick="lnkBackToLogin_Click">&larr; Back to log in</asp:LinkButton>
                            </p>
                        </asp:Panel>
                    </asp:View>

                    <%-- SUCCESS --%>
                    <asp:View ID="vOk" runat="server">
                        <div class="view on ok">
                            <div class="big">
                                <asp:Label ID="lblOkTitle" runat="server" />
                            </div>
                            <div class="note">
                                <asp:Label ID="lblOkMsg" runat="server" />
                            </div>

                            <asp:HyperLink ID="hlDash" runat="server" CssClass="btn full" />

                            <p class="alt-line">
                                <asp:LinkButton ID="lnkOkAgain" runat="server" CssClass="lk" CausesValidation="false" OnClick="lnkBackToLogin_Click">Use a different account</asp:LinkButton>
                            </p>
                        </div>
                    </asp:View>

                </asp:MultiView>
            </section>
        </main>
        <dialog id="countryDialog" class="country-dialog" aria-labelledby="countryDialogTitle" aria-describedby="countryDialogDescription">
            <div class="country-dialog-heading">
                <h2 id="countryDialogTitle">Select your country</h2>
                <button type="button" id="countryPickerClose" class="country-close" aria-label="Close country selection">&times;</button>
            </div>
            <p id="countryDialogDescription">Choose the country you call home.</p>
            <label class="l" for="countrySearch">Search countries</label>
            <input type="search" id="countrySearch" placeholder="Search by country name or code" autocomplete="off" />
            <p id="countryResultsCount" class="country-result-count" role="status" aria-live="polite"></p>
            <div id="countryResults" class="country-results" role="group" aria-label="Countries"></div>
            <p id="countryNoResults" class="country-empty" hidden>No countries found. Try another name or country code.</p>
        </dialog>
    </form>

    <script src="<%= ResolveUrl("~/F_JavaScripts/LoginSignupPage.js") %>?v=loading-state-3" defer></script>
</body>
</html>
