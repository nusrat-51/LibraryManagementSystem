using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;   // REQUIRED for ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================
// DATABASE CONFIG
// =========================
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryConnection")));


// =========================
// IDENTITY CONFIG
// =========================
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<LibraryContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();


// =========================
// SEED ROLES + DEFAULT USERS
// =========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // ROLES
    string[] roles = { "Admin", "Librarian", "Student" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ADMIN USER
    var adminEmail = "admin@gmail.com";
    var adminPass = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // LIBRARIAN USER
    var libEmail = "librarian@gmail.com";
    var libPass = "Librarian@123";

    var libUser = await userManager.FindByEmailAsync(libEmail);
    if (libUser == null)
    {
        libUser = new ApplicationUser
        {
            UserName = libEmail,
            Email = libEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(libUser, libPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(libUser, "Librarian");
    }

    // STUDENT USER
    var studentEmail = "student@gmail.com";
    var studentPass = "Student@123";

    var studentUser = await userManager.FindByEmailAsync(studentEmail);
    if (studentUser == null)
    {
        studentUser = new ApplicationUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(studentUser, studentPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(studentUser, "Student");
    }
}


// =========================
// MIDDLEWARE PIPELINE
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// =========================
// DEFAULT ROUTE
// =========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
