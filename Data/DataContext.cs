using Microsoft.EntityFrameworkCore;
using StoreRewards.Models;
using static System.Net.Mime.MediaTypeNames;

namespace StoreRewards.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Marketer> Marketers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Gender)
                .IsRequired()
                .HasConversion<string>();  // Converts enum to string in the database

            // One-to-Many relationship
            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Delete images when the product is deleted

            //modelBuilder.Entity<UserRole>()
            //    //.HasKey(ur => new { ur.UserId, ur.RoleId });  // Composite key
            //    .HasKey(ur => ur.Id);  // Primary key is Id (auto-incrementing)


            //modelBuilder.Entity<UserRole>()
            //    .HasOne(ur => ur.User)
            //    .WithMany(u => u.UserRoles)
            //    .HasForeignKey(ur => ur.UserId)
            //    .OnDelete(DeleteBehavior.Cascade); // Set the delete behavior to RESTRICT


            //modelBuilder.Entity<UserRole>()
            //    .HasOne(ur => ur.Role)
            //    .WithMany(r => r.UserRoles)
            //    .HasForeignKey(ur => ur.RoleId)
            //    .OnDelete(DeleteBehavior.Restrict); // Set the delete behavior to RESTRICT
        }

    }
}
