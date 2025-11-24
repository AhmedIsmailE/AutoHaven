<<<<<<< HEAD
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

=======
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
using AutoHaven.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ProjectDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connection"));
});
<<<<<<< HEAD

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ProjectDbContext>()
    .AddDefaultTokenProviders();

=======
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ProjectDbContext>();
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
<<<<<<< HEAD
=======
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
<<<<<<< HEAD
=======

>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

<<<<<<< HEAD
app.Run();
=======
app.Run();
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
