

namespace AzureTest.Infrastructure.Configuration;

public class IdentitySettings
{
    public const string SectionName = "IdentitySettings";

    public PasswordSettings Password { get; set; } = new();
    public LockoutSettings Lockout { get; set; } = new();
    public UserSettings User { get; set; } = new();
    public SignInSettings SignIn { get; set; } = new();

    // App identity settings (merged)
    public string RedirectAfterRegister { get; set; } = "/Auth/Login";
    public string RedirectAfterLogin { get; set; } = "/";
    public bool StayLoginAfterRegister { get; set; } = false;
    public List<string> RolesForRegister { get; set; } = new() { "User" };

    public class PasswordSettings
    {
        public bool RequireDigit { get; set; } = true;
        public int RequiredLength { get; set; } = 6;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public int RequiredUniqueChars { get; set; } = 1;
    }

    public class LockoutSettings
    {
        public bool AllowedForNewUsers { get; set; } = true;
        public int DefaultLockoutTimeSpanMinutes { get; set; } = 5;
        public int MaxFailedAccessAttempts { get; set; } = 5;
    }

    public class UserSettings
    {
        public string AllowedUserNameCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        public bool RequireUniqueEmail { get; set; } = true;
    }

    public class SignInSettings
    {
        public bool RequireConfirmedEmail { get; set; } = false;
        public bool RequireConfirmedPhoneNumber { get; set; } = false;
    }
}
