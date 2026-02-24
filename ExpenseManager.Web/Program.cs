using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Infrastructure.Data;
using ExpenseManager.Infrastructure.Repositories;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Services;
using ExpenseManager.Application.Mappings;
using ExpenseManager.Web.Middleware;
using ExpenseManager.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ExpenseManager.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity setup automatically handles Authentication and Cookie registration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// Configure the Identity Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Microsoft 365 Authentication integration
builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.GetSection("AzureAd").Bind(options);
        
        // Ensure OIDC signs directly into the main Identity cookie scheme
        options.SignInScheme = IdentityConstants.ApplicationScheme;
        
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            if (context.Properties.Items.TryGetValue("prompt", out var prompt))
            {
                context.ProtocolMessage.Prompt = prompt;
            }
            return Task.CompletedTask;
        };

        options.Events.OnTokenValidated = async context =>
        {
            var email = context.Principal?.FindFirstValue(ClaimTypes.Email) ?? 
                        context.Principal?.FindFirstValue("preferred_username");
            var name = context.Principal?.FindFirstValue("name") ?? "Microsoft User";

            Console.WriteLine($"[Auth] Microsoft Token Validated for: {email}");

            if (!string.IsNullOrEmpty(email))
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    Console.WriteLine($"[Auth] Microsoft login failed: User {email} not found in database.");
                    context.Response.Redirect($"/Account/AuthError?message={Uri.EscapeDataString("Unauthorized: User not found in database. Contact administrator.")}");
                    context.HandleResponse();
                    return;
                }

                if (!user.IsActive)
                {
                    Console.WriteLine($"[Auth] Microsoft login failed: User {email} is inactive.");
                    context.Response.Redirect($"/Account/AuthError?message={Uri.EscapeDataString("Unauthorized: Your account is inactive. Contact administrator.")}");
                    context.HandleResponse();
                    return;
                }
                
                // Sync FullName if it changed
                if (user.FullName != name && !string.IsNullOrEmpty(name))
                {
                    user.FullName = name;
                    await userManager.UpdateAsync(user);
                }
                
                // Add local claims to the principal that will be persisted in the cookie
                var claims = new List<Claim>
                {
                    new Claim("FullName", user.FullName ?? name),
                    new Claim(ClaimTypes.Role, (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User")
                };

                if (!string.IsNullOrEmpty(user.CompanyId))
                {
                    claims.Add(new Claim("CompanyId", user.CompanyId));
                }
                
                var appIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                context.Principal?.AddIdentity(appIdentity);
                Console.WriteLine($"[Auth] Successfully mapped local claims for: {email}");
            }
        };

        options.Events.OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[Auth] Authentication Failed: {context.Exception.Message}");
            context.Response.Redirect($"/Account/AuthError?message={Uri.EscapeDataString(context.Exception.Message)}");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    }, cookieOptions => {
        cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(24);
        cookieOptions.SlidingExpiration = true;
    }, OpenIdConnectDefaults.AuthenticationScheme, null); // cookieScheme: null to avoid re-registering Identity.Application

// Repositories
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletActivityRepository, WalletActivityRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IUserService, UserService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = 
        ForwardedHeaders.XForwardedFor | 
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();
    app.UseHsts();
}
else
{
    // For development, we might not want to force HTTPS if running only on an HTTP port
    // app.UseHttpsRedirection(); 
}

app.UseGlobalExceptionHandling();
app.UseStaticFiles();

app.UseRouting();
app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Seed logic will be called here
        await SeedData(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

app.Run();

async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
{
    // Roles — three-tier system
    if (!await roleManager.RoleExistsAsync("SuperAdmin")) await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!await roleManager.RoleExistsAsync("HR")) await roleManager.CreateAsync(new IdentityRole("HR"));

    // Remove old "User" role if it existed (migrate to new system)
    if (await roleManager.RoleExistsAsync("User"))
    {
        // Note: existing "User" role users should be reassigned manually
    }

    // SuperAdmin User — gopiprasanna@fdestech.com
    var superAdminEmail = "gopiprasanna@fdestech.com";
    var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
    if (superAdminUser == null)
    {
        superAdminUser = new ApplicationUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            FullName = "Gopi Prasanna Anumala",
            EmailConfirmed = true,
            IsActive = true,
            CompanyId = "FDES-TECH"
        };
        var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");

            // Seed Initial Wallet for SuperAdmin
            if (!context.Wallets.Any(w => w.UserId == superAdminUser.Id))
            {
                context.Wallets.Add(new ExpenseManager.Domain.Entities.Wallet
                {
                    UserId = superAdminUser.Id,
                    CompanyId = "FDES-TECH",
                    TotalCreditLimit = 50000,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }
    }
    else
    {
        // Ensure SuperAdmin role is assigned (upgrade from old Admin role)
        var currentRoles = await userManager.GetRolesAsync(superAdminUser);
        if (!currentRoles.Contains("SuperAdmin"))
        {
            // Remove old roles
            await userManager.RemoveFromRolesAsync(superAdminUser, currentRoles);
            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
        }
    }

    // Admin User — admin@expense.com (keep as Admin tier)
    var adminEmail = "admin@expense.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin Account",
            EmailConfirmed = true,
            IsActive = true,
            CompanyId = "FDES-TECH"
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");

        // Seed Initial Wallet for Admin
        if (!context.Wallets.Any(w => w.UserId == adminUser.Id))
        {
            context.Wallets.Add(new ExpenseManager.Domain.Entities.Wallet
            {
                UserId = adminUser.Id,
                CompanyId = "FDES-TECH",
                TotalCreditLimit = 30000,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var currentRoles = await userManager.GetRolesAsync(adminUser);
            await userManager.RemoveFromRolesAsync(adminUser, currentRoles);
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Seed Categories
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(new List<Category>
        {
            new Category { Name = "Hardware components", IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = "FDES-TECH" },
            new Category { Name = "Office thing", IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = "FDES-TECH" },
            new Category { Name = "Software Subscription", IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = "FDES-TECH" },
            new Category { Name = "Traveling", IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = "FDES-TECH" }
        });
        await context.SaveChangesAsync();
    }
}
