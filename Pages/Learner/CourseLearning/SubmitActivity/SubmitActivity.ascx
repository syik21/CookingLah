<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SubmitActivity.ascx.cs" Inherits="CookingLah.Pages.Learner.CourseLearning.SubmitActivity.SubmitActivity" %>

<asp:Panel
    ID="pnlActivityUnavailable"
    runat="server"
    Visible="false"
    CssClass="activity-empty-state">
    <h2>No practical activity is available</h2>
    <p>This course does not have a practical activity yet.</p>
</asp:Panel>

<asp:Panel
    ID="pnlActivity"
    runat="server"
    Visible="false"
    CssClass="activity-section">
    <header class="activity-heading">
        <p class="learning-eyebrow">Put it into practice</p>
        <h2><asp:Literal
                ID="litActivityTitle"
                runat="server"
                Mode="Encode" /></h2>
        <p>Show what you made and share a few notes with your Chef.</p>
    </header>

    <section class="activity-instructions" aria-labelledby="activityInstructionsTitle">
        <h3 id="activityInstructionsTitle">Activity instructions</h3>
        <asp:Literal
            ID="litActivityInstructions"
            runat="server"
            Mode="Encode" />
    </section>

    <asp:Panel
        ID="pnlActivityFeedback"
        runat="server"
        Visible="false"
        CssClass="activity-feedback"
        role="status">
        <asp:Literal
            ID="litActivityFeedback"
            runat="server"
            Mode="Encode" />
    </asp:Panel>

    <asp:Panel
        ID="pnlExistingSubmission"
        runat="server"
        Visible="false"
        CssClass="activity-submission-summary">
        <div class="activity-submission-heading">
            <div>
                <p class="activity-status-badge"><asp:Literal
                        ID="litSubmissionStatus"
                        runat="server"
                        Mode="Encode" /></p>
                <h3>Your submitted work</h3>
            </div>
            <p>Submitted <asp:Literal
                    ID="litSubmissionDate"
                    runat="server"
                    Mode="Encode" /></p>
        </div>
        <asp:Image
            ID="imgSubmittedWork"
            runat="server"
            CssClass="activity-submitted-image"
            Visible="false" />
        <p class="activity-submitted-notes"><asp:Literal
                ID="litSubmissionNotes"
                runat="server"
                Mode="Encode" /></p>
        <asp:Panel
            ID="pnlInstructorFeedback"
            runat="server"
            Visible="false"
            CssClass="activity-chef-feedback">
            <h4>Chef feedback</h4>
            <p><asp:Literal
                    ID="litInstructorFeedback"
                    runat="server"
                    Mode="Encode" /></p>
            <small>Reviewed <asp:Literal
                    ID="litReviewedDate"
                    runat="server"
                    Mode="Encode" /></small>
        </asp:Panel>
    </asp:Panel>

    <asp:Panel
        ID="pnlSubmissionForm"
        runat="server"
        CssClass="activity-submit-form"
        aria-labelledby="activitySubmitTitle">
        <h3 id="activitySubmitTitle"><asp:Literal
                ID="litSubmissionFormTitle"
                runat="server"
                Mode="Encode" /></h3>
        <p class="activity-form-hint">Upload a clear JPG or PNG image of your finished dish. Your latest submission will replace the previous one.</p>

        <div class="activity-upload-field">
            <asp:Label
                ID="lblActivityImage"
                runat="server"
                AssociatedControlID="fuActivityImage"
                Text="Photo of your dish" />
            <p class="activity-upload-hint">Choose a clear photo of your finished dish. JPG or PNG, up to 3 MB.</p>
            <asp:FileUpload
                ID="fuActivityImage"
                runat="server"
                CssClass="activity-file-input"
                accept="image/jpeg,image/png" />
            <p
                id="activityUploadFeedback"
                class="activity-upload-feedback"
                role="alert"
                hidden="hidden">
            </p>
            <div id="pnlActivityImagePreview" class="activity-image-preview" hidden="hidden">
                <img id="imgActivityPreview" src="" alt="Preview of your selected dish" />
                <div class="activity-preview-caption">
                    <span>Photo preview</span>
                    <button id="btnClearActivityImage" type="button" class="activity-clear-image">Choose another photo</button>
                </div>
            </div>
        </div>

        <div class="activity-notes-field">
            <asp:Label
                ID="lblActivityNotes"
                runat="server"
                AssociatedControlID="txtActivityDescription"
                Text="Notes about your dish (optional)" />
            <asp:TextBox
                ID="txtActivityDescription"
                runat="server"
                TextMode="MultiLine"
                Rows="4"
                MaxLength="1000"
                placeholder="What went well? What would you change next time?" />
            <small>Up to 1,000 characters.</small>
        </div>

        <asp:Button
            ID="btnSubmitActivity"
            runat="server"
            Text="Submit activity"
            CssClass="btn btn-primary activity-submit-button"
            OnClick="btnSubmitActivity_Click"
            CausesValidation="false" />
    </asp:Panel>
</asp:Panel>
