using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Revoulter.Core.Data;
using Revoulter.Core.Interfaces;
using Revoulter.Core.Models;
using Revoulter.Core.Services; // For ApplicationUser

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));  // <-- Changed from UseSqlServer

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity setup with custom ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+:";

    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// === THIS IS THE KEY FIX FOR 415 ERROR ===
builder.Services.AddControllers();

// This registers the JSON input formatter needed for [FromBody] or complex type binding in API controllers
// ===========================================
builder.Services.AddControllersWithViews(); // For regular MVC views
builder.Services.AddScoped<IArweaveUploader, MockArweaveUploader>();
builder.Services.AddScoped<IStoryProtocolRegistrar, MockStoryProtocolRegistrar>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // For Identity pages

app.Run();