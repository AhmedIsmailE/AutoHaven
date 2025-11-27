using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Models
{
    public class ProjectDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
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
        //public DbSet<SubscriptionPlanModel> SubscriptionPlans { get; set; }
        public DbSet<UserSubscriptionModel> UserSubscriptions { get; set; }

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
    }
}