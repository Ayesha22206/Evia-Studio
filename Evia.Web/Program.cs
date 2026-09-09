using Evia.Web.Data;
using Evia.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Database (SQL Server, per proposal's tools/technologies table) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Identity (Login / Register for the Customers stakeholder, + Roles for Admin) ---
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// --- AI outfit-image generation service (see Services/IAiDesignService.cs) ---
// Google Gemini Imagen 3 service for AI outfit image generation
//builder.Services.AddHttpClient<Evia.Web.Services.IAiDesignService, Evia.Web.Services.GeminiDesignService>();

// --- MVC (Controllers + Razor Views) ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // backs the scaffolded Identity UI (login/register pages)

// --- Session (used for the shopping cart before checkout) ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Products}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- Seed "Admin" role and a default admin account on startup ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string adminRole = "Admin";
    const string userRole = "User";

    // Ensure roles exist
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }
    if (!await roleManager.RoleExistsAsync(userRole))
    {
        await roleManager.CreateAsync(new IdentityRole(userRole));
    }

    // --- Seed Admin user (admin@gmail.com) ---
    const string adminEmail = "admin@gmail.com";
    const string adminPassword = "Admin@123"; // CHANGE THIS after first login in production
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "EVIA Admin",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, adminPassword);
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRole))
    {
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }

    // --- Seed Basic user (user@gmail.com) ---
    const string basicEmail = "user@gmail.com";
    const string basicPassword = "User@123"; // CHANGE THIS in production
    var basicUser = await userManager.FindByEmailAsync(basicEmail);
    if (basicUser == null)
    {
        basicUser = new ApplicationUser
        {
            UserName = basicEmail,
            Email = basicEmail,
            FullName = "EVIA User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(basicUser, basicPassword);
    }

    if (!await userManager.IsInRoleAsync(basicUser, userRole))
    {
        await userManager.AddToRoleAsync(basicUser, userRole);
    }
}

app.Run();
