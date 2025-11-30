using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.Repository;
using AutoHaven.Storage;  // ← ADD THIS
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ProjectDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connection"));
});

builder.Services.AddIdentity<ApplicationUserModel, IdentityRole<int>>()
    .AddEntityFrameworkStores<ProjectDbContext>()
    .AddDefaultTokenProviders();
// 1️ Add services
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //options.LoginPath = "/Account/LoginCustom";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// 2️ Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Role", "Admin"));
    options.AddPolicy("ProviderOnly", policy => policy.RequireClaim("Role", "Provider"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireClaim("Role", "Customer"));
    options.AddPolicy("AdminOrProvider", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim("Role", "Admin") ||
        context.User.HasClaim("Role", "Provider")
    ));

});
// ===== REPOSITORY REGISTRATIONS =====
builder.Services.AddScoped<ICarListingModelRepository, CarListingModelRepository>();
builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();
builder.Services.AddScoped<IUserSubscriptionModelRepository, UserSubscriptionModelRepository>();
builder.Services.AddScoped<ISubscriptionPlanModelRepository, SubscriptionPlanModelRepository>();
builder.Services.AddScoped<IReviewModelRepository, ReviewModelRepository>();
builder.Services.AddScoped<IFavouriteModelRepository, FavouriteRepository>();
builder.Services.AddScoped<ICarViewHistoryRepository, CarViewHistoryRepository>();


// ✅ ADD FILE STORAGE HERE - BEFORE builder.Build()
builder.Services.AddScoped<IFileStorage, FileSystemStorage>();

// ✅ BUILD AFTER ALL SERVICES ARE REGISTERED
var app = builder.Build();

// Configure the HTTP request pipeline.
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
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.Run();