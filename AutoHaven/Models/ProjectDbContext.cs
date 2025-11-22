using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Models
{
    public class ProjectDbContext : IdentityDbContext<ApplicationUser>
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
    }
}