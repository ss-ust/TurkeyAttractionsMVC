using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurkeyAttractionsMVC.Models;

namespace TurkeyAttractionsMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserAttraction> UserAttractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<City>()
                .Property(c => c.ImageUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<Attraction>()
                .Property(a => a.ImageUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<Attraction>()
                .HasOne(a => a.City)
                .WithMany(c => c.Attractions)
                .HasForeignKey(a => a.CityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Attraction)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.AttractionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAttraction>()
                .HasKey(ua => new { ua.UserId, ua.AttractionId });

            modelBuilder.Entity<UserAttraction>()
                .HasOne(ua => ua.Attraction)
                .WithMany(a => a.UserAttractions)
                .HasForeignKey(ua => ua.AttractionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAttraction>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
