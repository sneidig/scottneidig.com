using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
