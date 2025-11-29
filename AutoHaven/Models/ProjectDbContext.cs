using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoHaven.Models
{
    public class ProjectDbContext : IdentityDbContext<ApplicationUserModel, IdentityRole<int>, int>
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarModel> Cars { get; set; }
        public DbSet<CarListingModel> CarListings { get; set; }
        public DbSet<CarViewHistoryModel> CarViewHistories { get; set; }
        public DbSet<CarImageModel> CarImages { get; set; }
        public DbSet<FavouriteModel> Favourites { get; set; }
        public DbSet<ReviewModel> Reviews { get; set; }
        public DbSet<SubscriptionPlanModel> SubscriptionPlans { get; set; }
        public DbSet<UserSubscriptionModel> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== RELATIONSHIP CONFIGS ====================
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

            // ==================== USER INDEXES ====================
            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedUserName_Unique");

            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedEmail_Unique");

            // NATIONAL ID UNIQUE INDEX (allows multiple NULLs)
            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.NationalId)
                .IsUnique()
                .HasFilter("[NationalId] IS NOT NULL")
                .HasDatabaseName("IX_AspNetUsers_NationalId_Unique");

            // ==================== SEED SUBSCRIPTION PLANS ====================
            modelBuilder.Entity<SubscriptionPlanModel>().HasData(
                new SubscriptionPlanModel
                {
                    SubscriptionPlanId = 1,
                    SubscriptionName = "Free",
                    MaxCarListing = 0,
                    FeatureSlots = 0,
                    PricePerMonth = 0,
                    tier = SubscriptionPlanModel.Tiers.Free
                },
                new SubscriptionPlanModel
                {
                    SubscriptionPlanId = 2,
                    SubscriptionName = "Starter",
                    MaxCarListing = 5,
                    FeatureSlots = 0,
                    PricePerMonth = 10,
                    tier = SubscriptionPlanModel.Tiers.Starter
                },
                new SubscriptionPlanModel
                {
                    SubscriptionPlanId = 3,
                    SubscriptionName = "Pro",
                    MaxCarListing = 20,
                    FeatureSlots = 3,
                    PricePerMonth = 25,
                    tier = SubscriptionPlanModel.Tiers.Pro
                },
                new SubscriptionPlanModel
                {
                    SubscriptionPlanId = 4,
                    SubscriptionName = "Elite",
                    MaxCarListing = 50,
                    FeatureSlots = 10,
                    PricePerMonth = 50,
                    tier = SubscriptionPlanModel.Tiers.Elite
                }
            );
        }
    }
}