<%@ Page Title="My Profile | Cooking LAH" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="true" CodeBehind="ProfilePage.aspx.cs" Inherits="CookingLah.Pages.Shared.Profile.ProfilePage" %>

<%-- Profile page stylesheet --%>
<asp:Content
    ID="DashboardHead"
    ContentPlaceHolderID="ProfileHead"
    runat="server">
    <link href="<%= ResolveUrl("~/Pages/Shared/Profile/Profile.css") %>" rel="stylesheet" />
</asp:Content>

<%-- Main profile page content --%>
<asp:Content
    ID="ProfileContent"
    ContentPlaceHolderID="ProfileContent"
    runat="server">
    <asp:ScriptManager ID="ProfileScriptManager" runat="server" />
    <div class="profile-page">
        <input type="hidden" name="profileCsrf" value="<%: CsrfToken %>" />
        <a class="profile-skip" href="#profileMain">Skip to profile</a>

        <div id="profileMain" class="container profile-main" tabindex="-1">
            <%-- Page heading --%>
            <div class="profile-heading">
                <div>
                    <p class="profile-breadcrumb">My account / Profile</p>
                    <h1>My profile</h1>
                    <p>A little about the person behind the apron.</p>
                </div>
                <span class="profile-stamp" aria-hidden="true">Made with<br />a little flavour.</span>
            </div>

            <noscript>
                <p class="scrap-paper">Enable JavaScript to edit your profile, change your password, and upload photos.</p>
            </noscript>

            <div class="profile-layout">
                <%-- Left sidebar: profile photo and navigation --%>
                <aside class="profile-sidebar" aria-label="Profile overview">
                    <section class="scrap-paper scrap-tape profile-card" aria-labelledby="profileName">
                        <span class="scrap-label">The cook behind the apron</span>

                        <button
                            id="editProfilePhoto"
                            class="profile-avatar-button"
                            type="button"
                            aria-label="Change profile photo"
                            aria-haspopup="dialog"
                            aria-controls="profilePhotoDialog">
                            <img
                                id="profileAvatar"
                                data-default-avatar="<%= ResolveUrl("~/F_Assets/mascot.png") %>"
                                class="profile-avatar"
                                src="<%: ProfileImageUrl %>"
                                alt="Your profile photo"
                                width="144"
                                height="144" />
                        </button>

                        <h2 id="profileName"><%: FullName %></h2>
                        <p id="profileHandle">@<%: Username %></p>
                        <p class="profile-photo-help">Click your photo to change it.</p>
                        <p id="photoSaveFeedback" class="profile-feedback" role="status"><%: PhotoFeedback %></p>
                    </section>

                    <%-- Jump links to profile settings --%>
                    <nav class="profile-section-nav" aria-label="Profile sections">
                        <a href="#personalDetails">Personal details <span aria-hidden="true">01</span></a>
                        <a href="#accountSecurity">Account security <span aria-hidden="true">02</span></a>
                    </nav>

                    <div class="profile-note">
                        <p class="handwritten-note">Every great dish starts with you.</p>
                        <p>Your profile puts a familiar face to your cooking journey.</p>
                    </div>
                </aside>

                <%-- Right side: personal details and security --%>
                <div class="profile-sections">
                    <%-- Personal details form --%>
                    <section id="personalDetails" class="scrap-paper profile-details" aria-labelledby="detailsTitle">
                        <div class="profile-section-heading">
                            <span class="profile-number" aria-hidden="true">01</span>
                            <div>
                                <h2 id="detailsTitle">Personal details</h2>
                                <p>Your name, your notebook.</p>
                            </div>
                        </div>

                        <%-- Fields start disabled; the edit button enables them. --%>
                        <fieldset
                            id="personalDetailsFields"
                            class="profile-fields profile-edit-fields"
                            aria-labelledby="detailsTitle"
                            disabled="disabled">
                            <div class="profile-field profile-field-wide">
                                <label for="fullName">Full name (required)</label>
                                <input
                                    id="fullName"
                                    name="fullName"
                                    value="<%: FullName %>"
                                    autocomplete="name"
                                    maxlength="100"
                                    placeholder="Your full name"
                                    required="required" />
                            </div>

                            <div class="profile-field">
                                <label for="username">Username (required)</label>
                                <input
                                    id="username"
                                    name="username"
                                    value="<%: Username %>"
                                    autocomplete="username"
                                    maxlength="50"
                                    placeholder="yourusername"
                                    required="required" />
                                <small>Your name around the kitchen.</small>
                            </div>

                            <div class="profile-field">
                                <label for="email">Email address (required)</label>
                                <input
                                    id="email"
                                    name="email"
                                    value="<%: Email %>"
                                    type="email"
                                    autocomplete="email"
                                    maxlength="100"
                                    placeholder="you@example.com"
                                    required="required" />
                            </div>
                        </fieldset>

                        <%-- Edit, cancel and save buttons --%>
                        <div class="profile-actions">
                            <p id="detailsState">Select Edit profile to update your details.</p>
                            <button
                                id="editDetails"
                                type="button"
                                class="btn btn-primary"
                                aria-controls="personalDetailsFields">Edit profile</button>
                            <button
                                id="resetDetails"
                                type="button"
                                class="btn btn-secondary"
                                hidden>Cancel</button>
                            <button
                                id="previewDetails"
                                type="button"
                                class="btn btn-primary"
                                hidden>Save changes</button>
                        </div>
                        <p id="detailsFeedback" class="profile-feedback" role="status"><%: DetailsFeedback %></p>
                    </section>

                    <%-- Account security and password update --%>
                    <asp:UpdatePanel
                        ID="upnlPassword"
                        runat="server"
                        UpdateMode="Conditional"
                        ChildrenAsTriggers="true">
                        <ContentTemplate>
                            <section id="accountSecurity" class="scrap-paper profile-security" aria-labelledby="securityTitle">
                                <div class="profile-section-heading">
                                    <span class="profile-number" aria-hidden="true">02</span>
                                    <div>
                                        <h2 id="securityTitle">Account security</h2>
                                        <p>Keep your kitchen key safe.</p>
                                    </div>
                                </div>

                                <p class="profile-security-note">Only you should know your password. Verify your current password before choosing a new one.</p>

                                <asp:Panel
                                    ID="pnlPasswordStart"
                                    runat="server"
                                    CssClass="profile-actions">
                                    <asp:Button
                                        ID="btnStartPasswordChange"
                                        runat="server"
                                        Text="Change password"
                                        CssClass="btn btn-primary"
                                        OnClick="btnStartPasswordChange_Click" />
                                </asp:Panel>

                                <asp:Panel
                                    ID="pnlPasswordVerify"
                                    runat="server"
                                    Visible="false"
                                    CssClass="profile-fields profile-edit-fields">
                                    <div class="profile-field profile-field-wide">
                                        <label for="txtCurrentPassword">Current password</label>
                                        <asp:TextBox
                                            ID="txtCurrentPassword"
                                            runat="server"
                                            ClientIDMode="Static"
                                            TextMode="Password"
                                            autocomplete="current-password"
                                            MaxLength="128" />
                                    </div>
                                    <div class="profile-actions">
                                        <asp:Button
                                            ID="btnVerifyCurrentPassword"
                                            runat="server"
                                            Text="Verify password"
                                            CssClass="btn btn-primary"
                                            OnClick="btnVerifyCurrentPassword_Click" />
                                        <asp:Button
                                            ID="btnCancelPasswordVerify"
                                            runat="server"
                                            Text="Cancel"
                                            CssClass="btn btn-secondary"
                                            OnClick="btnCancelPasswordChange_Click"
                                            CausesValidation="false" />
                                    </div>
                                </asp:Panel>

                                <asp:Panel
                                    ID="pnlPasswordNew"
                                    runat="server"
                                    Visible="false"
                                    CssClass="profile-fields profile-edit-fields">
                                    <div class="profile-field">
                                        <label for="txtNewPassword">New password</label>
                                        <asp:TextBox
                                            ID="txtNewPassword"
                                            runat="server"
                                            ClientIDMode="Static"
                                            TextMode="Password"
                                            autocomplete="new-password"
                                            MaxLength="128" />
                                        <small>Use at least 8 characters including a letter, number and symbol (maximum 72 UTF-8 bytes).</small>
                                    </div>
                                    <div class="profile-field">
                                        <label for="txtConfirmPassword">Confirm new password</label>
                                        <asp:TextBox
                                            ID="txtConfirmPassword"
                                            runat="server"
                                            ClientIDMode="Static"
                                            TextMode="Password"
                                            autocomplete="new-password"
                                            MaxLength="128" />
                                    </div>
                                    <div class="profile-actions">
                                        <asp:Button
                                            ID="btnSaveNewPassword"
                                            runat="server"
                                            Text="Save new password"
                                            CssClass="btn btn-primary"
                                            OnClick="btnSaveNewPassword_Click" />
                                        <asp:Button
                                            ID="btnCancelPasswordChange"
                                            runat="server"
                                            Text="Cancel"
                                            CssClass="btn btn-secondary"
                                            OnClick="btnCancelPasswordChange_Click"
                                            CausesValidation="false" />
                                    </div>
                                </asp:Panel>
                                <p
                                    id="passwordFeedback"
                                    class="profile-feedback profile-password-feedback<%= PasswordFeedbackIsError ? " is-error" : " is-success" %>"
                                    role="status"
                                    aria-live="polite"><%: PasswordFeedback %></p>
                            </section>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <asp:UpdateProgress
                        ID="passwordProgress"
                        runat="server"
                        AssociatedUpdatePanelID="upnlPassword"
                        DisplayAfter="150">
                        <ProgressTemplate>
                            <p class="profile-feedback" role="status" aria-live="polite">Processing your request…</p>
                        </ProgressTemplate>
                    </asp:UpdateProgress>

                    <section id="deleteProfile" class="scrap-paper profile-delete" aria-labelledby="deleteProfileTitle">
                        <div class="profile-section-heading">
                            <span class="profile-number" aria-hidden="true">03</span>
                            <div>
                                <h2 id="deleteProfileTitle">Delete profile</h2>
                                <p>Manage the end of your cooking journey.</p>
                            </div>
                        </div>
                        <p><%: DeleteDescription %></p>
                        <% if (CanDeleteProfile) { %>
                        <button
                            id="openDeleteProfile"
                            type="button"
                            class="btn profile-delete-button"
                            aria-haspopup="dialog"
                            aria-controls="deleteProfileDialog"><%: DeleteActionLabel %></button>
                        <% } %>
                        <p class="profile-feedback" role="status"><%: DeleteFeedback %></p>
                    </section>

                    <p class="profile-footnote handwritten-note">A little practice. A lot of flavour.</p>
                </div>
            </div>
        </div>

        <% if (CanDeleteProfile) { %>
        <dialog
            id="deleteProfileDialog"
            class="profile-delete-dialog"
            aria-labelledby="deleteDialogTitle"
            aria-describedby="deleteDialogDescription"
            data-reopen="<%= string.IsNullOrEmpty(DeleteFeedback) ? "false" : "true" %>">
            <h2 id="deleteDialogTitle"><%: DeleteActionLabel %>?</h2>
            <p id="deleteDialogDescription"><%: DeleteDescription %> You will be signed out immediately.</p>
            <div class="profile-field">
                <label for="deletePassword">Current password (required)</label>
                <input
                    id="deletePassword"
                    name="deletePassword"
                    type="password"
                    autocomplete="current-password"
                    maxlength="128"
                    aria-describedby="deleteProfileError" />
            </div>
            <label class="profile-delete-confirm"><input
                    id="confirmDeleteProfile"
                    name="confirmDeleteProfile"
                    type="checkbox"
                    value="yes" /> <span>I understand and want to <%: DeleteActionLabel.ToLowerInvariant() %>.</span></label>
            <p id="deleteProfileError" class="profile-feedback" role="alert"><%: DeleteFeedback %></p>
            <div class="profile-delete-actions">
                <button
                    id="cancelDeleteProfile"
                    type="button"
                    class="btn btn-secondary"
                    autofocus>Cancel</button>
                <button id="confirmDeleteButton" type="button" class="btn profile-delete-button"><%: DeleteActionLabel %></button>
            </div>
        </dialog>
        <% } %>

        <%-- Popup for uploading or removing a profile photo --%>
        <dialog
            id="profilePhotoDialog"
            class="profile-photo-dialog"
            aria-labelledby="photoDialogTitle"
            aria-describedby="photoHelp">
            <button
                id="closePhotoDialog"
                class="profile-photo-close"
                type="button"
                aria-label="Close photo popup"
                autofocus>
                <span aria-hidden="true">&times;</span>
            </button>
            <h2 id="photoDialogTitle">Profile photo</h2>
            <img
                id="photoPreview"
                class="profile-avatar"
                src="<%: ProfileImageUrl %>"
                alt="Selected profile photo preview"
                width="144"
                height="144" />
            <p class="profile-photo-help" id="photoHelp">Choose a JPG or PNG, up to 2 MB.<br />Saved photos appear on your profile.</p>
            <label class="profile-upload-label" for="profilePhoto">Choose photo</label>
            <input
                id="profilePhoto"
                name="profilePhoto"
                type="file"
                accept="image/jpeg,image/png"
                aria-describedby="photoHelp photoFeedback" />
            <button
                id="removePhoto"
                class="profile-text-button"
                type="button"
                hidden>Remove photo</button>
            <p id="photoFeedback" class="profile-feedback" role="status">
            </p>
            <div class="profile-photo-actions">
                <button
                    id="savePhoto"
                    class="btn btn-primary"
                    type="button"
                    disabled>Save</button>
            </div>
        </dialog>
    </div>
</asp:Content>

<%-- Profile page JavaScript --%>
<asp:Content
    ID="ProfileScripts"
    ContentPlaceHolderID="ProfileScripts"
    runat="server">
    <script src="<%= ResolveUrl("~/F_JavaScripts/PasswordStrength.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Shared/Profile/Profile.js") %>" defer></script>
</asp:Content>
