<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseReview.ascx.cs" Inherits="CookingLah.Pages.Chef.ManageCourse.Review.CourseReview" %>
<section class="course-reviews-panel" aria-labelledby="courseReviewHeading">
    <header>
        <p class="course-review-kicker">Course Reviews</p>
        <h2 id="courseReviewHeading">Learner feedback</h2>
    </header>
    <asp:Panel
        ID="pnlSummary"
        runat="server"
        CssClass="course-review-summary">
        <div class="course-review-average"><span>Average Rating</span><strong><asp:Literal
                    ID="litAverageRating"
                    runat="server"
                    Mode="Encode" /></strong><p><asp:Literal
                    ID="litReviewCount"
                    runat="server"
                    Mode="Encode" /></p>
        </div>
        <div class="course-review-distribution" aria-label="Rating distribution">
            <asp:Repeater ID="rptDistribution" runat="server">
                <ItemTemplate>
                    <div class="course-review-bar-row"><span><%#: Eval("Rating") %> &#9733;</span><div class="course-review-bar">
                            <asp:Panel
                                runat="server"
                                CssClass="course-review-bar-fill"
                                Width='<%# System.Web.UI.WebControls.Unit.Percentage(Convert.ToDouble(Eval("Percent"))) %>' />
                        </div><strong><%#: Eval("Count") %></strong></div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlReviews" runat="server">
        <div class="course-review-rows">
            <asp:Repeater ID="rptReviews" runat="server">
                <ItemTemplate>
                    <article class="course-review-row">
                        <div class="course-review-row-heading">
                            <div>
                                <h3><%#: Eval("LearnerName") %></h3>
                                <p>Reviewed on <time datetime='<%# ReviewDateValue(Eval("CreatedAt")) %>'><%#: ReviewDate(Eval("CreatedAt")) %></time></p>
                            </div>
                            <p class="course-review-rating" aria-label='<%# Eval("Rating") + " out of 5 stars" %>'><span aria-hidden="true"><%# StarText(Eval("Rating")) %></span> <%#: Eval("Rating") %> / 5</p>
                        </div>
                        <p class="course-review-text"><%#: Eval("Comment") %></p>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <asp:Panel
            ID="pnlPagination"
            runat="server"
            CssClass="course-review-pagination"><asp:Button
                ID="btnPrevious"
                runat="server"
                Text="&#8249; Previous"
                CssClass="btn"
                OnClick="btnPrevious_Click"
                CausesValidation="false" /><asp:Label ID="lblPageStatus" runat="server" /><asp:Button
                ID="btnNext"
                runat="server"
                Text="Next &#8250;"
                CssClass="btn"
                OnClick="btnNext_Click"
                CausesValidation="false" /></asp:Panel>
    </asp:Panel>
    <asp:Panel
        ID="pnlEmpty"
        runat="server"
        Visible="false"
        CssClass="course-review-empty">
        <h3>No reviews yet.</h3>
        <p>Learner reviews will appear here after completed learners share their feedback.</p>
    </asp:Panel>
</section>
