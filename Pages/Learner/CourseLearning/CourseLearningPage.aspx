<%@ Page Title="Learn Course | Cooking LAH" Language="C#" MasterPageFile="~/Pages/Learner/Learner.Master" AutoEventWireup="true" CodeBehind="CourseLearningPage.aspx.cs" Inherits="CookingLah.Pages.Learner.CourseLearning.CourseLearningPage" %>
<%@ Register Src="~/Pages/Learner/CourseLearning/Lesson/LessonContent.ascx" TagPrefix="learning" TagName="LessonContent" %>
<%@ Register Src="~/Pages/Learner/CourseLearning/QuizSection/QuizSection.ascx" TagPrefix="learning" TagName="QuizSection" %>
<%@ Register Src="~/Pages/Learner/CourseLearning/SubmitActivity/SubmitActivity.ascx" TagPrefix="learning" TagName="SubmitActivity" %>
<%@ Import Namespace="System.Web" %>

<asp:Content ID="LearningHead" ContentPlaceHolderID="LearnerHead" runat="server">
    <link href="<%= ResolveUrl("~/Pages/Learner/CourseLearning/CourseLearningPage.css?v=20261027d") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Learner/CourseLearning/Lesson/LessonContent.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Learner/CourseLearning/QuizSection/QuizSection.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Pages/Learner/CourseLearning/SubmitActivity/SubmitActivity.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="LearningContent" ContentPlaceHolderID="LearnerContent" runat="server">
    <div class="course-learning-page">
        <asp:Panel ID="pnlCourseUnavailable" runat="server" Visible="false" CssClass="learning-unavailable">
            <h1>Course unavailable</h1>
            <p><asp:Literal ID="litAccessMessage" runat="server" Mode="Encode" /></p>
            <a class="btn btn-primary" href="<%= ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.aspx") %>">Back to My Courses</a>
        </asp:Panel>

        <asp:Panel ID="pnlLearningContent" runat="server" Visible="false">
            <div class="learning-page-bar">
                <a class="learning-back" href="<%= ResolveUrl("~/Pages/Learner/MyCourse/LearnerMyCourse.aspx") %>">
                    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M19 12H5m7 7-7-7 7-7" /></svg>
                    Back to My Courses
                </a>
                <span id="courseStatusBadge" runat="server" class="learning-course-status" aria-label="Course status"></span>
            </div>

            <asp:Image ID="imgCourseThumbnail" runat="server" CssClass="learning-course-thumbnail" />
            <header class="learning-course-header">
                
                <div class="learning-course-heading">
                    <p class="learning-eyebrow">Your learning kitchen</p>
                    <h1><asp:Literal ID="litCourseTitle" runat="server" Mode="Encode" /></h1>
                    <p class="learning-course-meta">
                        <asp:Literal ID="litCourseMeta" runat="server" Mode="Encode" />
                    </p>
                    <p class="learning-chef-line">Created by <strong><asp:Literal ID="litChefName" runat="server" Mode="Encode" /></strong></p>
                </div>
                <div class="learning-status-column">
                    <section class="learning-progress" aria-labelledby="progressTitle">
                        <div class="learning-progress-heading">
                            <div>
                                <p class="learning-eyebrow">Keep going</p>
                                <h2 id="progressTitle">Your progress</h2>
                            </div>
                            <strong class="learning-progress-value"><asp:Literal ID="litProgressPercent" runat="server" Mode="Encode" />%</strong>
                        </div>
                        <div id="courseProgressBar" runat="server" class="learning-progress-track" role="progressbar"
                            aria-label="Course progress" aria-valuemin="0" aria-valuemax="100">
                            <span id="courseProgressFill" runat="server" class="learning-progress-fill"></span>
                        </div>
                        <p class="learning-progress-summary"><asp:Literal ID="litProgressSummary" runat="server" Mode="Encode" /></p>
                    </section>

                    <asp:Panel ID="pnlCourseSubmitted" runat="server" Visible="false" CssClass="learning-course-complete">
                        <div class="learning-complete-message">
                            <div>
                                <strong>Course submitted</strong>
                                <span>You have completed the learning requirements. Your Practical Activity is waiting for Chef approval.</span>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlCourseComplete" runat="server" Visible="false" CssClass="learning-course-complete">
                        <div class="learning-complete-message">
                            <span class="learning-complete-icon" aria-hidden="true">&#10003;</span>
                            <div>
                                <strong>Course completed!</strong>
                                <span>Your Practical Activity has been approved. <asp:Literal ID="litCompletionCourse" runat="server" Mode="Encode" /></span>
                            </div>
                        </div>
                        <div class="learning-complete-actions">
                            <asp:HyperLink ID="lnkBackToOverview" runat="server" CssClass="btn btn-secondary">Back to Course Overview</asp:HyperLink>
                            <asp:HyperLink ID="lnkCompletionReview" runat="server" CssClass="btn btn-primary" />
                        </div>
                    </asp:Panel>
                </div>
            </header>

            <asp:Panel ID="pnlLearningFeedback" runat="server" Visible="false" CssClass="learning-feedback" role="status">
                <asp:Literal ID="litLearningFeedback" runat="server" Mode="Encode" />
            </asp:Panel>

            <div class="learning-mobile-curriculum-toggle">
                <button id="btnToggleCurriculum" type="button" class="btn btn-secondary"
                    aria-expanded="false" aria-controls="courseCurriculum">
                    Course content
                    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="m6 9 6 6 6-6" /></svg>
                </button>
            </div>

            <div class="learning-layout">
                <aside id="courseCurriculum" class="learning-curriculum" aria-label="Course curriculum">
                    <div class="curriculum-heading">
                        <p class="learning-eyebrow">Your course</p>
                        <h2>Course content</h2>
                        <asp:Literal ID="litLessonCount" runat="server" Mode="Encode" />
                    </div>

                    <section class="curriculum-section" aria-labelledby="curriculumLessonsTitle">
                        <h3 id="curriculumLessonsTitle">Lessons</h3>
                        <asp:Repeater ID="rptCourseLessons" runat="server" OnItemCommand="rptCourseLessons_ItemCommand">
                            <ItemTemplate>
                                <asp:Panel runat="server" CssClass='<%# LessonItemClass(Eval("LessonID")) %>'>
                                    <span class="curriculum-state" aria-hidden="true"><%# LessonStateIcon(Eval("LessonID")) %></span>
                                    <span class="curriculum-lesson-copy">
                                        <span class="curriculum-lesson-order">Lesson <%#: Eval("LessonOrder") %></span>
                                        <asp:LinkButton ID="btnSelectLesson" runat="server"
                                            Text='<%# HttpUtility.HtmlEncode(Convert.ToString(Eval("Title"))) %>'
                                            CssClass="curriculum-lesson-title" CommandName="SelectLesson"
                                            CommandArgument='<%# Eval("LessonID") %>' CausesValidation="false" />
                                    </span>
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:Repeater>
                    </section>

                    <section class="curriculum-section" aria-labelledby="curriculumQuizTitle">
                        <h3 id="curriculumQuizTitle">Assessment</h3>
                        <asp:Panel ID="pnlQuizCurriculumItem" runat="server" CssClass="curriculum-special-item">
                            <span class="curriculum-state" aria-hidden="true"><asp:Literal ID="litQuizState" runat="server" /></span>
                            <asp:LinkButton ID="btnOpenQuiz" runat="server" Text="Course quiz"
                                OnClick="btnOpenQuiz_Click" CausesValidation="false" />
                        </asp:Panel>
                    </section>

                    <section class="curriculum-section" aria-labelledby="curriculumActivityTitle">
                        <h3 id="curriculumActivityTitle">Practical</h3>
                        <asp:Panel ID="pnlActivityCurriculumItem" runat="server" CssClass="curriculum-special-item">
                            <span class="curriculum-state" aria-hidden="true"><asp:Literal ID="litActivityState" runat="server" /></span>
                            <asp:LinkButton ID="btnOpenActivity" runat="server" Text="Practical activity"
                                OnClick="btnOpenActivity_Click" CausesValidation="false" />
                        </asp:Panel>
                    </section>
                </aside>

                <div class="learning-workspace">
                    <asp:Panel ID="pnlLesson" runat="server">
                        <asp:Panel ID="pnlLessonEmpty" runat="server" Visible="false" CssClass="learning-empty">
                            <h2>Lessons are on the way</h2>
                            <p>This published course does not have any lessons available yet.</p>
                        </asp:Panel>
                        <learning:LessonContent ID="LessonContent1" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlQuiz" runat="server" Visible="false">
                        <learning:QuizSection ID="QuizSection1" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlActivity" runat="server" Visible="false">
                        <learning:SubmitActivity ID="SubmitActivity1" runat="server" />
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>

<asp:Content ID="LearningScripts" ContentPlaceHolderID="LearnerScripts" runat="server">
    <script src="<%= ResolveUrl("~/Pages/Learner/CourseLearning/CourseLearningPage.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Learner/CourseLearning/Lesson/LessonContent.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Learner/CourseLearning/QuizSection/QuizSection.js") %>" defer></script>
    <script src="<%= ResolveUrl("~/Pages/Learner/CourseLearning/SubmitActivity/SubmitActivity.js") %>" defer></script>
</asp:Content>
