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
        public DbSet<CarViewHistoryModel> CarViewHistories { get; set; } // To Collect History Posts Per User
        public DbSet<CarImageModel> CarImages { get; set; }
        public DbSet<FavouriteModel> Favourites { get; set; }
        public DbSet<ReviewModel> Reviews { get; set; }
        public DbSet<SubscriptionPlanModel> SubscriptionPlans { get; set; }
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
            modelBuilder.Entity<FavouriteModel>()
            .HasOne(f => f.CarListing)
            .WithMany(c => c.Favourites)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.NoAction);
            // Important: create unique index on NormalizedUserName and NormalizedEmail to match Identity normalization
            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedUserName_Unique");

            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedEmail_Unique");

            // EF Core way to make unique index that allows multiple NULLs
            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.NationalId)
                .IsUnique()
                .HasFilter("[NationalId] IS NOT NULL") // فقط تحقق الفريد للقيم غير null
                .HasDatabaseName("IX_AspNetUsers_NationalId_Unique");
            // Create password hasher
            var hasher = new PasswordHasher<ApplicationUserModel>();

            // Admin Users
            var admin1 = new ApplicationUserModel
            {
                Id = 1,
                UserName = "MadPixel",
                NormalizedUserName = "MADPIXEL",
                PhoneNumber = "01121386733",
                Email = "ahmed@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                NormalizedEmail = "AHMED@GMAIL.COM",
                Role = ApplicationUserModel.RoleEnum.Admin,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            admin1.PasswordHash = hasher.HashPassword(admin1, "123456@Aasd"); // password

            var admin2 = new ApplicationUserModel
            {
                Id = 2,
                UserName = "Arsany",
                NormalizedUserName = "ARSANY",
                PhoneNumber = "01289938194",
                Email = "arsany@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                NormalizedEmail = "ARSANY@GMAIL.COM",
                Role = ApplicationUserModel.RoleEnum.Admin,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            admin2.PasswordHash = hasher.HashPassword(admin2, "123456Aa.");

            var admin3 = new ApplicationUserModel
            {
                Id = 3,
                UserName = "Mohamed",
                NormalizedUserName = "MOHAMED",
                PhoneNumber= "01012488360",
                Email = "mohamed@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                NormalizedEmail = "MOHAMED@GMAIL.COM",
                Role = ApplicationUserModel.RoleEnum.Admin,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            admin3.PasswordHash = hasher.HashPassword(admin3, "Aa@123456");

            var admin4 = new ApplicationUserModel
            {
                Id = 4,
                UserName = "Omar",
                NormalizedUserName = "OMAR",
                Email = "omar@gmail.com",
                NormalizedEmail = "OMAR@GMAIL.COM",
                PhoneNumber = "01111031724",
                SecurityStamp = Guid.NewGuid().ToString(),
                Role = ApplicationUserModel.RoleEnum.Admin,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            admin4.PasswordHash = hasher.HashPassword(admin4, "Aa@123456");

            // Seed them
            modelBuilder.Entity<ApplicationUserModel>().HasData(admin1, admin2, admin3, admin4);
            modelBuilder.Entity<UserSubscriptionModel>().HasData(
               
                new UserSubscriptionModel
                {
                    UserSubscriptionId = 1,
                    UserId = 1, 
                    PlanId = 4, 
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1), 
                    CurrentStatus = UserSubscriptionModel.Status.Active
                },

            
                new UserSubscriptionModel
                {
                    UserSubscriptionId = 2,
                    UserId = 2,
                    PlanId = 4,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    CurrentStatus = UserSubscriptionModel.Status.Active
                },

               
                new UserSubscriptionModel
                {
                    UserSubscriptionId = 3,
                    UserId = 3, 
                    PlanId = 4,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    CurrentStatus = UserSubscriptionModel.Status.Active
                },

               
                new UserSubscriptionModel
                {
                    UserSubscriptionId = 4,
                    UserId = 4, 
                    PlanId = 4,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    CurrentStatus = UserSubscriptionModel.Status.Active
                }
            );
        }
    }
}