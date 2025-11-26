//using AutoHaven.Models;
//using AutoHaven.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<ProjectDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("connection"));
//});
////builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ProjectDbContext>();
//builder.Services.AddIdentity<ApplicationUser, IdentityRole<string>>()
//    .AddEntityFrameworkStores<ProjectDbContext>();
//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthentication();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Account}/{action=Index}/{id?}");

//app.Run();

//using AutoHaven.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<ProjectDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("connection"));
//});

//builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
//    .AddEntityFrameworkStores<ProjectDbContext>()
//    .AddDefaultTokenProviders();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Account}/{action=Index}/{id?}");

//app.Run();

using AutoHaven.Models;
using AutoHaven.IRepository;
using AutoHaven.Repository;  // ← Your implementations are here!
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ProjectDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ProjectDbContext>()
    .AddDefaultTokenProviders();

// ===== REPOSITORY REGISTRATIONS =====
builder.Services.AddScoped<ICarListingModelRepository, CarListingModelRepository>();
builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();
builder.Services.AddScoped<IUserSubscriptionModelRepository, UserSubscriptionModelRepository>();
builder.Services.AddScoped<IReviewModelRepository, ReviewModelRepository>();
builder.Services.AddScoped<IFavouriteModelRepository, FavouriteRepository>();
var app = builder.Build();

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
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();