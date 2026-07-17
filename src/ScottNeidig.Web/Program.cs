using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Configuration;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Connection string comes from user-secrets in dev and host config in production,
// never from a committed file. Fail fast and loudly if it is missing or blank:
// an empty string is not null, so a plain null check would let it through and
// surface later as a confusing EF error.
var connection = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException(
        "Missing connection string 'Default'. Set it with: dotnet user-secrets set \"ConnectionStrings:Default\" \"...\"");
}

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connection));

builder.Services.Configure<AdminUserOptions>(
    builder.Configuration.GetSection(AdminUserOptions.SectionName));

// Unlike AdminUser these are not secrets, so they live in appsettings and are safe to commit.
builder.Services.Configure<ImageOptions>(
    builder.Configuration.GetSection(ImageOptions.SectionName));

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectPointService, ProjectPointService>();
builder.Services.AddScoped<IProjectImageService, ProjectImageService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IImageStorage, ImageStorage>();

// Stateless and holds nothing per-request, so one instance serves everything.
builder.Services.AddSingleton<IImageProcessor, ImageProcessor>();

// Roles are registered now even though nothing uses them yet, so adding one later
// is a code change rather than a migration.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // There is one human account and no self-service registration, so the value here
        // is throttling brute force, not enforcing password theatre.
        options.Password.RequiredLength = 12;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/account/login";
    options.AccessDeniedPath = "/admin/account/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Area route must come first, otherwise /admin/... is swallowed by the default route.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, logger);
}

app.Run();
