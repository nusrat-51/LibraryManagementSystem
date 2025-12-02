using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;   // <<--- IMPORTANT

var builder = WebApplication.CreateBuilder(args);

// ===========================
//  DATABASE
// ===========================
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryConnection")));

// ===========================
//  IDENTITY (Users + Roles)
// ===========================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // password rules – simple rakhlam
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<LibraryContext>()
    .AddDefaultTokenProviders();

// login/logout path config
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ===========================
//  MVC
// ===========================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ===========================
//  PIPELINE
// ===========================
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

// default route → sobar age login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
