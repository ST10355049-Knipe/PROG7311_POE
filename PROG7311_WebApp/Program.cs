using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using PROG7311_WebApp.Data;
using PROG7311_WebApp.Models;
using PROG7311_WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Configure ASP.NET Core Identity
//Used Microsoft Learn to understand configuration for ASP.NET Core Identity in my program.cs file, specifically in the Configure identity section
//https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // Password settings
    //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false; 
    options.Password.RequireUppercase = false;      
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Ensure emails are unique
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders() // For password reset, email confirmation tokens
.AddDefaultUI();

// Configure application cookie
//https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // How long the login session lasts
    options.LoginPath = "/Account/Login";       // If not authenticated, redirect here
    options.AccessDeniedPath = "/Account/AccessDenied"; // If authorised but access denied to a resource
    options.SlidingExpiration = true; // Resets expiration on activity
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IProductService, ProductService>(); // Register the ProductService for dependency injection

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Seeding database...");

        await DbInitialiser.Initialse(services);

        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); 

app.Run();