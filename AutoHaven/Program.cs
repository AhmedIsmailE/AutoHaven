using AutoHaven.Models;
using AutoHaven.IRepository;
using AutoHaven.Repository;
using AutoHaven.Storage;  // ← ADD THIS
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
    pattern: "{controller=Account}/{action=Home}/{id?}");

app.Run();