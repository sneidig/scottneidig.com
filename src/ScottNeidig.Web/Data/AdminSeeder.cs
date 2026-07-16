using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ScottNeidig.Web.Configuration;

namespace ScottNeidig.Web.Data;

/// <summary>
/// Creates the single admin account on startup if it does not already exist.
/// There is no registration page by design, so this is the only way in.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var options = services.GetRequiredService<IOptions<AdminUserOptions>>().Value;

        // Fail fast rather than silently starting with no way to log in.
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "AdminUser:Email and AdminUser:Password must be configured. Set them with user-secrets.");
        }

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        if (await userManager.FindByEmailAsync(options.Email) is not null)
        {
            return;
        }

        var user = new IdentityUser
        {
            UserName = options.Email,
            Email = options.Email,
            // No confirmation flow exists, so the seeded account is trusted by definition.
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, options.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create the admin user: {errors}");
        }

        logger.LogInformation("Seeded admin account {Email}", options.Email);
    }
}
