<<<<<<< HEAD
﻿//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//namespace AutoHaven.Models
//{
//    public class ProjectDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
//    {
//        public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
//        : base(options)
//        {
//        }
//        public DbSet<CarModel> Cars { get; set; }
//        public DbSet<CarListingModel> CarListings { get; set; }
//        public DbSet<CarImageModel> CarImages { get; set; }
//        public DbSet<FavouriteModel> Favourites { get; set; }
//        public DbSet<ReviewModel> Reviews { get; set; }
//        public DbSet<SubscriptionPlanModel> SubscriptionPlans { get; set; }
//        public DbSet<UserSubscriptionModel> UserSubscriptions { get; set; }


//    }
//}





using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Models
{
    public class ProjectDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
=======
﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Models
{
    public class ProjectDbContext : IdentityDbContext<ApplicationUser>
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarModel> Cars { get; set; }
        public DbSet<CarListingModel> CarListings { get; set; }
        public DbSet<CarImageModel> CarImages { get; set; }
        public DbSet<FavouriteModel> Favourites { get; set; }
        public DbSet<ReviewModel> Reviews { get; set; }
        public DbSet<SubscriptionPlanModel> SubscriptionPlans { get; set; }
        public DbSet<UserSubscriptionModel> UserSubscriptions { get; set; }

<<<<<<< HEAD
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReviewModel>()
                .HasOne(r => r.CarListing)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FavouriteModel>()
                .HasOne(f => f.CarListing)
                .WithMany(c => c.Favourites)
                .HasForeignKey(f => f.ListingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
=======
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<UserModel>()
        //        .HasMany(u => u.Favourites)
        //        .WithOne(f => f.User)
        //        .HasForeignKey(f => f.UserId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<UserModel>()
        //        .HasMany(u => u.Reviews)
        //        .WithOne(r => r.User)
        //        .HasForeignKey(r => r.UserId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<UserModel>()
        //        .HasMany(u => u.CarListings)
        //        .WithOne(cl => cl.User)
        //        .HasForeignKey(cl => cl.ChangedBy)
        //        .OnDelete(DeleteBehavior.Restrict);

        //    modelBuilder.Entity<UserModel>()
        //        .HasMany(u => u.UserSubscriptions)
        //        .WithOne(us => us.User)
        //        .HasForeignKey(us => us.UserId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<CarModel>()
        //        .HasMany(c => c.CarListings)
        //        .WithOne(cl => cl.Car)
        //        .HasForeignKey(cl => cl.CarId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<CarListingModel>()
        //        .HasMany(cl => cl.CarImages)
        //        .WithOne(ci => ci.CarListing)
        //        .HasForeignKey(ci => ci.ListingId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<CarListingModel>()
        //        .HasMany(cl => cl.Reviews)
        //        .WithOne(r => r.CarListing)
        //        .HasForeignKey(r => r.ListingId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<CarListingModel>()
        //        .HasMany(cl => cl.Favourites)
        //        .WithOne(f => f.CarListing)
        //        .HasForeignKey(f => f.ListingId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    modelBuilder.Entity<SubscriptionPlanModel>()
        //        .HasMany(sp => sp.UserSubscriptions)
        //        .WithOne(us => us.SubscriptionPlan)
        //        .HasForeignKey(us => us.SubscriptionPlanId)
        //        .OnDelete(DeleteBehavior.Cascade);
        //}
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    {
        //     optionsBuilder.UseSqlServer("Server=DESKTOP-L8GAIPI\\SQLEXPRESS;Database=DepiR3G4D;TrusedConnection=true;Trust Server Certificate=True");


        //    base.OnConfiguring(optionsBuilder);
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=.;Database=AutoHavenDB;" +
        //        "Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;");
        //    base.OnConfiguring(optionsBuilder);
        //}
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
    }
}