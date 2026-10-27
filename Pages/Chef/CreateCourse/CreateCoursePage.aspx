<%@ Page Title="Create Course | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Chef/Chef.Master" AutoEventWireup="true" CodeBehind="CreateCoursePage.aspx.cs" Inherits="CookingLah.Pages.Chef.CreateCourse.CreateCoursePage" %>
<%@ Register Src="~/Pages/Shared/CountryPicker/CountryPicker.ascx" TagPrefix="shared" TagName="CountryPicker" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="ChefHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Shared/CountryPicker/CountryPicker.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="ChefContent" runat="server">
<main class="create-course-page" data-create-course="" data-unsaved="false" aria-labelledby="createCourseTitle">
    <a class="create-course-back" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>">&larr; Back to My Courses</a>

    <header class="create-course-heading">
        <div><p class="create-course-kicker">Start a new course</p><h1 id="createCourseTitle">Create Course</h1><p>Add the essentials now. You&rsquo;ll build the recipe, lessons, quiz, and practical activity in Manage Course.</p></div>
        <div class="create-course-step" aria-label="Creation progress"><strong>1</strong><span>Basic information<br /><small>Then manage content</small></span></div>
    </header>

    <asp:Panel ID="pnlErrors" runat="server" CssClass="create-course-errors" role="alert" Visible="false" tabindex="-1">
        <h2>Please check these details</h2><ul><asp:Repeater ID="rptErrors" runat="server"><ItemTemplate><li><%#: Container.DataItem %></li></ItemTemplate></asp:Repeater></ul>
    </asp:Panel>

    <div class="create-course-layout">
        <section class="create-course-form" aria-labelledby="basicInformationTitle">
            <div class="create-course-section-heading"><span aria-hidden="true">01</span><div><h2 id="basicInformationTitle">Basic information</h2><p>Fields marked * are required.</p></div></div>

            <div class="create-course-field">
                <asp:Label runat="server" AssociatedControlID="txtCourseTitle" Text="Course title *" />
                <span>Use a clear name that tells learners what they will cook.</span>
                <asp:TextBox ID="txtCourseTitle" runat="server" MaxLength="150" placeholder="e.g. Ikan Bakar Basics" autocomplete="off" />
            </div>
            <div class="create-course-field">
                <asp:Label runat="server" AssociatedControlID="txtDescription" Text="Course description *" />
                <span>Describe the dish and the skills learners will practise.</span>
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="5" MaxLength="2000" placeholder="What will learners make and learn?" />
                <small class="create-course-count"><span data-description-count>0</span> / 2000</small>
            </div>
            <div class="create-course-field-grid">
                <div class="create-course-field"><asp:Label runat="server" AssociatedControlID="ddlCategory" Text="Category *" /><asp:DropDownList ID="ddlCategory" runat="server" /></div>
                <div class="create-course-field"><asp:Label runat="server" AssociatedControlID="ddlDifficulty" Text="Difficulty *" /><asp:DropDownList ID="ddlDifficulty" runat="server"><asp:ListItem Text="Select difficulty" Value="" /><asp:ListItem Text="Beginner" /><asp:ListItem Text="Intermediate" /><asp:ListItem Text="Advanced" /></asp:DropDownList></div>
            </div>

            <section class="traditional-food-section" data-category-food-section hidden="hidden" aria-labelledby="traditionalFoodTitle">
                <div class="traditional-food-heading"><span class="traditional-food-icon" aria-hidden="true"><svg viewBox="0 0 24 24" focusable="false"><path d="M4 11.5h16l-1.2 6.1a2 2 0 0 1-2 1.6H7.2a2 2 0 0 1-2-1.6L4 11.5Z"/><path d="M3 11.5h18M8 8.5c0-1.2 1-1.5 1-2.7M12 8.5c0-1.2 1-1.5 1-2.7M16 8.5c0-1.2 1-1.5 1-2.7"/></svg></span><div><h3 id="traditionalFoodTitle">Traditional Food</h3><p>Connect this course to the dish it teaches.</p></div></div>
                <div data-traditional-food-fields>
                    <div class="create-course-field"><asp:Label runat="server" AssociatedControlID="rblFoodCultureMode" Text="Choose an option" /><asp:RadioButtonList ID="rblFoodCultureMode" runat="server" CssClass="traditional-food-options" RepeatDirection="Horizontal" RepeatLayout="Flow"><asp:ListItem Text="Select an existing food" Value="Existing" Selected="True" /><asp:ListItem Text="Add a new food" Value="New" /></asp:RadioButtonList></div>
                    <div class="create-course-field" data-food-existing-fields><asp:Label runat="server" AssociatedControlID="ddlFoodCulture" Text="Traditional Food *" /><span>Type a dish name, then choose it from the results.</span><div class="traditional-food-picker" data-food-picker hidden="hidden"><input type="search" data-food-search placeholder="Search dishes" autocomplete="off" aria-autocomplete="list" aria-haspopup="listbox" /><div class="traditional-food-results" data-food-results hidden="hidden"><p data-food-search-status role="status" aria-live="polite"></p><div data-food-options role="listbox" aria-label="Traditional food results"></div><p data-food-empty hidden="hidden">No dishes found. Try another name.</p></div></div><asp:DropDownList ID="ddlFoodCulture" runat="server" /></div>
                    <div class="traditional-food-new-fields" data-food-new-fields hidden="hidden"><div class="create-course-field"><asp:Label runat="server" AssociatedControlID="txtNewDishName" Text="Dish name *" /><asp:TextBox ID="txtNewDishName" runat="server" MaxLength="150" /></div><div class="create-course-field"><shared:CountryPicker ID="foodCountryPicker" runat="server" LabelText="Country *" /></div><p>A new Traditional Food will be added to the Food Map when the course is saved.</p></div>
                </div>
            </section>

            <div class="create-course-field create-course-thumbnail">
                <asp:Label runat="server" AssociatedControlID="fuCourseThumbnail" Text="Course thumbnail" />
                <span>Optional. Choose a clear JPG or PNG of the finished dish, up to 3 MB.</span>
                <label class="create-course-upload" for="<%= fuCourseThumbnail.ClientID %>"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v14H4zM4 15l4-4 4 4 3-3 5 5M9 9h.01" /></svg><strong>Choose course image</strong><small data-file-name>No file selected</small></label>
                <asp:FileUpload ID="fuCourseThumbnail" runat="server" accept="image/jpeg,image/png" data-max-file-mb="3" />
                <asp:Image ID="imgThumbnail" runat="server" AlternateText="Selected course thumbnail preview" />
            </div>
        </section>

        <aside class="create-course-guide" aria-labelledby="nextTitle">
            <p class="create-course-note">Your course starts as a private draft.</p>
            <h2 id="nextTitle">What happens next?</h2>
            <ol><li><strong>Save the basics</strong><span>We create your course draft.</span></li><li><strong>Build your content</strong><span>Add the recipe, lessons, quiz, and activity in Manage Course.</span></li><li><strong>Submit when ready</strong><span>Your course stays private until it is reviewed.</span></li></ol>
        </aside>
    </div>

    <footer class="create-course-actions">
        <p><strong>Ready to continue?</strong><span>Your draft remains private while you finish it.</span></p>
        <div><a class="btn" href="<%= ResolveUrl("~/Pages/Chef/MyCourse/ChefMyCourse.aspx") %>">Cancel</a><asp:Button ID="btnSaveNext" runat="server" Text="Save &amp; next" CssClass="btn btn-primary" OnClick="btnSaveNext_Click" CausesValidation="false" /></div>
    </footer>
</main>
</asp:Content>
<asp:Content ID="ScriptsContent" ContentPlaceHolderID="ChefScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Shared/CountryPicker/CountryPicker.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/F_JavaScripts/UploadValidation.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Chef/CreateCourse/CreateCoursePage.js") %>" defer></script>
</asp:Content>
