using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;   // ⭐ REQUIRED for ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------ DB CONTEXT ------------
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryConnection")));

// ------------ IDENTITY ------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>       // ⭐ FIXED
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<LibraryContext>()
    .AddDefaultTokenProviders();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ------------ SEED ROLES + USERS ------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();   // ⭐ FIXED

    // 1) Roles
    string[] roleNames = { "Admin", "Librarian", "Student" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 2) Admin user
    var adminEmail = "admin@gmail.com";
    var adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // 3) Librarian user
    var librarianEmail = "librarian@gmail.com";
    var librarianPassword = "Librarian@123";

    var librarianUser = await userManager.FindByEmailAsync(librarianEmail);
    if (librarianUser == null)
    {
        librarianUser = new ApplicationUser
        {
            UserName = librarianEmail,
            Email = librarianEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(librarianUser, librarianPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(librarianUser, "Librarian");
        }
    }

    // 4) Student user
    var studentEmail = "student@gmail.com";
    var studentPassword = "Student@123";

    var studentUser = await userManager.FindByEmailAsync(studentEmail);
    if (studentUser == null)
    {
        studentUser = new ApplicationUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(studentUser, studentPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(studentUser, "Student");
        }
    }
}

// ------------ PIPELINE ------------
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
