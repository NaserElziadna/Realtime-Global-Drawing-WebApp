using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignalRDrawingApp.Data;
using SignalRDrawingApp.Data.UnitOfWork;
using SignalRDrawingApp.Hubs;
using SignalRDrawingApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Add authorization policies for two roles only: User and Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// Register UnitOfWork and repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure SignalR hubs
app.MapHub<DrawingHub>("/drawingHub");
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRolesAndAdminUser(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles and admin user.");
    }
}

app.Run();

// Method to seed roles and admin user
static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    // Create only two roles: User and Admin
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Created role: {RoleName}", roleName);
            }
            else
            {
                logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Role {RoleName} already exists", roleName);
        }
    }

    // Create admin user
    var adminEmail = "admin@drawing.app";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        logger.LogInformation("Creating admin user with email: {Email}", adminEmail);
        
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Nickname = "Administrator",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "admin123");
        if (result.Succeeded)
        {
            logger.LogInformation("Admin user created successfully");
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Admin role assigned to admin user");
            }
            else
            {
                logger.LogError("Failed to assign Admin role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
    else
    {
        logger.LogInformation("Admin user already exists with email: {Email}", adminEmail);
        
        // Ensure admin user is active and has correct properties
        var needsUpdate = false;
        if (!adminUser.IsActive)
        {
            adminUser.IsActive = true;
            needsUpdate = true;
            logger.LogInformation("Admin user was inactive, setting to active");
        }
        
        if (!adminUser.EmailConfirmed)
        {
            adminUser.EmailConfirmed = true;
            needsUpdate = true;
            logger.LogInformation("Admin user email was not confirmed, setting to confirmed");
        }
        
        if (needsUpdate)
        {
            await userManager.UpdateAsync(adminUser);
            logger.LogInformation("Admin user updated");
        }
        
        // Ensure admin user has Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin role added to existing admin user");
        }
    }
}
