namespace UserService.Domain.Constants;

public class EmailPurpose
{
    public static class MagicLinkPurpose
    {
        public const string EmailVerification = "EmailVerification";
        public const string PasswordlessLogin = "PasswordlessLogin";
        public const string PasswordReset = "PasswordReset";
        public const string ChangeEmail = "ChangeEmail";
    }
}
