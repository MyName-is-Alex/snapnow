namespace snapnow.Utils;

public static class EmailBody
{
    public static string GetEmailConfirmationBody(string userMail, string url)
    {
        return
            $"Hello, {userMail}!<br />Thank you for creating an account on our platform.<br />Access the link below to confirm your account.<br /><a href='{url}'>Click here to confirm your email address.</a>";
    }
}