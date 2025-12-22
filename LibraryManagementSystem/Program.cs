using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// =====================
// DATABASE
// =====================
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// IDENTITY
// =====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<LibraryContext>()
.AddDefaultTokenProviders();

// ✅ Ensure unauthorized redirects go to your Account/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// =====================
// MVC
// =====================
builder.Services.AddControllersWithViews();
// QuestPDF license (some versions require this)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// DI
builder.Services.AddScoped<IReceiptPdfService, ReceiptPdfService>();
builder.Services.AddScoped<FineCalculator>();


// ✅ Register FineCalculator only ONCE
builder.Services.AddScoped<FineCalculator>();

var app = builder.Build();

// =====================
// MIDDLEWARE ORDER
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Must be here (after routing, before map routes)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
