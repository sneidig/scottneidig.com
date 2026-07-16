namespace ScottNeidig.Web.Configuration;

/// <summary>
/// Credentials for the single admin account, seeded on startup.
/// Bound from user-secrets in dev and host config in production. Never committed.
/// </summary>
public class AdminUserOptions
{
    public const string SectionName = "AdminUser";

    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
