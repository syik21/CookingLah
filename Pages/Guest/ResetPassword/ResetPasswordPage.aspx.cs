using CookingLah.F_Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CookingLah.Pages.Guest.ResetPassword
{
    public partial class ResetPasswordPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ValidateResetToken(Request.QueryString["token"]))
                    ShowResetForm();
                else
                    ShowInvalidTokenState();
            }
        }

        // Validate that the reset token is unused and not expired.
        private bool ValidateResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128)
                return false;
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            using (SqlCommand command = new SqlCommand(@"
                SELECT TOP (1) t.ResetTokenID
                FROM PasswordResetTokens t
                INNER JOIN Users u ON u.UserID=t.UserID
                WHERE t.TokenHash=@tokenHash AND t.IsUsed=0
                  AND t.ExpiresAt>SYSUTCDATETIME() AND u.AccountStatus='Active'", connection))
            {
                command.Parameters.Add("@tokenHash", SqlDbType.NVarChar, 255).Value = PasswordResetTokenManager.HashToken(token);
                connection.Open();
                return command.ExecuteScalar() != null;
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            lblResetError.Visible = false;
            string password = txtNewPassword.Text;
            string confirmation = txtConfirmPassword.Text;
            if (password.Length == 0 || confirmation.Length == 0)
            {
                ShowError("Enter and confirm your new password.");
                return;
            }
            if (password != confirmation)
            {
                ShowError("The passwords do not match. Please enter them again.");
                return;
            }
            if (!PasswordManager.MeetsPasswordPolicy(password))
            {
                ShowError("Use at least 8 characters including a letter, number and symbol, and at most 72 UTF-8 bytes for your password.");
                return;
            }

            try
            {
                if (!ResetPassword(Request.QueryString["token"], password))
                {
                    ShowInvalidTokenState();
                    return;
                }
                ShowSuccessState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Password reset failed: {0}", ex.Message);
                ShowError("We could not update your password. Please try again.");
            }
        }

        // Update the user's password and invalidate reset tokens.
        private bool ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128)
                return false;
            using (SqlConnection connection = DatabaseManager.ConnectDB())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int? userId = null;
                    using (SqlCommand command = new SqlCommand(@"
                        SELECT TOP (1) t.UserID
                        FROM PasswordResetTokens t WITH (UPDLOCK,HOLDLOCK)
                        INNER JOIN Users u WITH (UPDLOCK,HOLDLOCK) ON u.UserID=t.UserID
                        WHERE t.TokenHash=@tokenHash AND t.IsUsed=0
                          AND t.ExpiresAt>SYSUTCDATETIME() AND u.AccountStatus='Active'", connection, transaction))
                    {
                        command.Parameters.Add("@tokenHash", SqlDbType.NVarChar, 255).Value = PasswordResetTokenManager.HashToken(token);
                        object result = command.ExecuteScalar();
                        if (result != null)
                            userId = Convert.ToInt32(result);
                    }
                    if (!userId.HasValue)
                    {
                        transaction.Rollback();
                        return false;
                    }
                    using (SqlCommand command = new SqlCommand("UPDATE Users SET Password=@password,UpdatedAt=GETDATE() WHERE UserID=@userId", connection, transaction))
                    {
                        command.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = PasswordManager.HashPassword(newPassword);
                        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId.Value;
                        if (command.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("The reset account no longer exists.");
                    }
                    using (SqlCommand command = new SqlCommand("UPDATE PasswordResetTokens SET IsUsed=1 WHERE UserID=@userId AND IsUsed=0", connection, transaction))
                    {
                        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId.Value;
                        if (command.ExecuteNonQuery() < 1)
                        {
                            throw new InvalidOperationException("The reset token could not be invalidated.");
                        }
                    }
                    transaction.Commit();
                    return true;
                }
            }
        }

        private void ShowResetForm()
        {
            pnlResetForm.Visible = true;
            pnlInvalid.Visible = false;
            pnlSuccess.Visible = false;
        }

        private void ShowInvalidTokenState()
        {
            pnlResetForm.Visible = false;
            pnlInvalid.Visible = true;
            pnlSuccess.Visible = false;
        }

        private void ShowSuccessState()
        {
            pnlResetForm.Visible = false;
            pnlInvalid.Visible = false;
            pnlSuccess.Visible = true;
        }

        private void ShowError(string message)
        {
            ShowResetForm();
            lblResetError.Text = message;
            lblResetError.Visible = true;
        }
    }
}
