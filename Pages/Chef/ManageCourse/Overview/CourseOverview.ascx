<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseOverview.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Overview.CourseOverview" %>
<%@ Register Src="~/Pages/Shared/CountryPicker/CountryPicker.ascx" TagPrefix="shared" TagName="CountryPicker" %>
<section class="course-overview">
    <div class="mc-section-heading"><h2>Course Overview</h2><asp:Button ID="btnEdit" runat="server" Text="Edit Information" CssClass="btn" OnClick="btnEdit_Click" /></div>
    <asp:Repeater ID="rptOverview" runat="server"><ItemTemplate>
        <article class="card overview-details">
            <asp:Image runat="server" CssClass="overview-image" ImageUrl='<%# CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail"))) %>' Visible='<%# !string.IsNullOrEmpty(CookingLah.Pages.Chef.CreateCourse.CourseBuilderData.MediaUrl(Convert.ToString(Eval("Thumbnail")))) %>' AlternateText="Course thumbnail" />
            <h3><%#: Eval("Title") %></h3>
            <dl><div><dt>Category</dt><dd><%#: Eval("CategoryName") %></dd>
            </div><div><dt>Difficulty</dt><dd><%#: Eval("DifficultyLevel") %></dd>
            </div><div><dt>Traditional Food</dt><dd><%#: Eval("FoodDisplayName") %></dd>
            </div><div><dt>Status</dt><dd><%#: Eval("Status") %></dd>
            </div><div><dt>Created</dt><dd><%#: Eval("CreatedAt", "{0:dd MMM yyyy}") %></dd>
            </div><div><dt>Updated</dt><dd><%#: Eval("UpdatedAt") == DBNull.Value ? "Not updated yet" : Eval("UpdatedAt", "{0:dd MMM yyyy}") %></dd></div></dl>
            <h3>Description</h3><p class="mc-copy"><%#: Eval("Description") %></p>
        </article>
    </ItemTemplate></asp:Repeater>
    <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="card mc-modal" data-title="Edit Course">
        <h2>Edit Course</h2>
        <div class="mc-field">
        <asp:Label runat="server" AssociatedControlID="txtTitle" Text="Course Title *" />
        <asp:TextBox ID="txtTitle" runat="server" MaxLength="150" />
        </div><div class="mc-field">
        <asp:Label runat="server" AssociatedControlID="txtDescription" Text="Description *" />
        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" />
        </div><div class="mc-field">
        <asp:Label runat="server" AssociatedControlID="fuThumbnail" Text="Thumbnail" />
        <asp:FileUpload ID="fuThumbnail" runat="server" accept="image/png,image/jpeg" data-image-input="" data-max-file-mb="3" /></div>
        <div class="mc-field"><asp:Label runat="server" AssociatedControlID="ddlCategory" Text="Category *" /><asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" /></div>
        <div class="mc-field">
        <asp:Label runat="server" AssociatedControlID="ddlDifficulty" Text="Difficulty *" />
        <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select"><asp:ListItem>Beginner</asp:ListItem><asp:ListItem>Intermediate</asp:ListItem><asp:ListItem>Advanced</asp:ListItem></asp:DropDownList></div>
        <section class="mc-traditional-food" data-category-food-section hidden="hidden" aria-labelledby="mcTraditionalFoodTitle">
            <div class="mc-traditional-heading">
                <span class="mc-traditional-icon" aria-hidden="true">
                    <svg viewBox="0 0 24 24" focusable="false"><path d="M4 11.5h16l-1.2 6.1a2 2 0 0 1-2 1.6H7.2a2 2 0 0 1-2-1.6L4 11.5Z"/><path d="M3 11.5h18M8 8.5c0-1.2 1-1.5 1-2.7M12 8.5c0-1.2 1-1.5 1-2.7M16 8.5c0-1.2 1-1.5 1-2.7"/></svg>
                </span>
                <div><h3 id="mcTraditionalFoodTitle">Traditional Food</h3><p>Select the dish covered in this course.</p></div>
            </div>
            <div class="mc-traditional-fields" data-traditional-food-fields>
                <div class="mc-field">
                    <asp:Label runat="server" AssociatedControlID="rblFoodCultureMode" Text="How would you like to continue?" />
                    <asp:RadioButtonList ID="rblFoodCultureMode" runat="server" CssClass="mc-traditional-options"
                        RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem Text="Select existing Traditional Food" Value="Existing" Selected="True" />
                        <asp:ListItem Text="Add new Traditional Food" Value="New" />
                    </asp:RadioButtonList>
                </div>
                <div class="mc-field" data-food-existing-fields>
                    <asp:Label runat="server" AssociatedControlID="ddlFoodCulture" Text="Traditional Food *" />
                    <asp:DropDownList ID="ddlFoodCulture" runat="server" CssClass="form-select" />
                </div>
                <div class="mc-traditional-new-fields" data-food-new-fields hidden="hidden">
                    <div class="mc-field">
                        <asp:Label runat="server" AssociatedControlID="txtNewDishName" Text="Dish Name *" />
                        <asp:TextBox ID="txtNewDishName" runat="server" MaxLength="150" />
                    </div>
                    <div class="mc-field">
                        <shared:CountryPicker ID="foodCountryPicker" runat="server" LabelText="Country *" />
                    </div>
                    <p class="text-muted">A new Traditional Food will be added to the Food Map when you save the course.</p>
                </div>
            </div>
        </section>
        <p class="text-muted">Images: JPG or PNG, up to 3 MB.</p><img class="mc-image-preview" data-image-preview="" alt="Selected course thumbnail" hidden />
        <div class="mc-actions">
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" data-cancel="" />
        <asp:Button ID="btnSaveCourse" runat="server" Text="Apply Changes" CssClass="btn btn-primary" OnClick="btnSaveCourse_Click" /></div>
    </asp:Panel>
</section>
