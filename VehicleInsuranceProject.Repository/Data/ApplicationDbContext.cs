using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define your DbSets for each entity that maps to a database table
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<CoverageLevel> CoverageLevels { get; set; } // DbSet for the new CoverageLevel entity
        public DbSet<Claim> Claims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Always call base method for IdentityDbContext configurations

            // Configure the relationship between Customer and IdentityUser
            builder.Entity<Customer>()
                .HasOne(c => c.IdentityUser)
                .WithMany()
                .HasForeignKey(c => c.IdentityUserId)
                .IsRequired();

            builder.Entity<Customer>()
               .Property(c => c.IsActive)
               .HasDefaultValue(true);

            // Configure the relationship between Vehicle and Customer
            builder.Entity<Vehicle>()
                .HasOne(v => v.Customer!)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .IsRequired();

            // Configure how VehicleType enum is stored in the database as a string
            builder.Entity<Vehicle>()
                .Property(v => v.vehicleType)
                .HasConversion<string>();


            // Configure the relationship between Policy and Vehicle
            builder.Entity<Policy>()
                .HasOne(p => p.Vehicle!)
                .WithMany()
                .HasForeignKey(p => p.vehicleId)
                .IsRequired();

            // Configure how PolicyStatus enum is stored in the database as a string
            builder.Entity<Policy>()
                .Property(p => p.policyStatus)
                .HasConversion<string>();

            // Configure the relationship between Policy and CoverageLevel
            builder.Entity<Policy>()
                .HasOne(p => p.CoverageLevel)
                .WithMany()
                .HasForeignKey(p => p.CoverageLevelId)
                .IsRequired(false);

            // Configure decimal column types for CoverageLevel properties
            builder.Entity<CoverageLevel>()
                .Property(cl => cl.PremiumMultiplier)
                .HasColumnType("decimal(18,2)");

            builder.Entity<CoverageLevel>()
                .Property(cl => cl.CoverageMultiplier)
                .HasColumnType("decimal(18,2)");

            // *** CRITICAL FIX: Explicitly define the one-to-many relationship from Policy to Claims ***
            builder.Entity<Policy>()
                .HasMany(p => p.Claims) // Policy has many Claims
                .WithOne(c => c.Policy!) // Each Claim has one Policy
                .HasForeignKey(c => c.policyId) // The foreign key in the Claims table
                .IsRequired() // A Claim must be associated with a Policy
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of policies when claims exist

            // Existing configuration for Claim entity
            builder.Entity<Claim>()
                .Property(cl => cl.claimStatus)
                .HasConversion<string>();

            builder.Entity<Claim>()
                .Property(cl => cl.claimAmount)
                .HasColumnType("decimal(10, 2)");
        }
    }
}
