using Event_booking.Api.Models;
using EventBooking.Api.Models;
using EventBooking.Api.Models.Event_booking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fix decimal precision warnings
            modelBuilder.Entity<Event>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            // Fix cascade delete conflict - prevent multiple cascade paths
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Entertainment" },
                new Category { Id = 2, Name = "Sports" },
                new Category { Id = 3, Name = "Education" },
                new Category { Id = 4, Name = "Business" },
                new Category { Id = 5, Name = "Technology" }
            );

            // Seed Users - Use fixed GUIDs for consistency
            var user1Id = new Guid("11111111-1111-1111-1111-111111111111");
            var organizerId = new Guid("22222222-2222-2222-2222-222222222222");
            var adminId = new Guid("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = user1Id,
                    Name = "John Doe",
                    Email = "john@example.com",
                    PasswordHash = "hashedpassword1",
                    Role = UserRole.Attendee,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    Id = organizerId,
                    Name = "Event Organizer",
                    Email = "organizer@example.com",
                    PasswordHash = "hashedpassword2",
                    Role = UserRole.Organizer,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    Id = adminId,
                    Name = "Event Organizer",
                    Email = "organizer@example.com",
                    PasswordHash = "hashedpassword2",
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed Events
            var eventId = new Guid("33333333-3333-3333-3333-333333333333");
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    id = eventId,
                    Title = "Tech Conference 2025",
                    Description = "Annual technology conference",
                    Location = "Convention Center",
                    Date = new DateTime(2025, 3, 15),
                    OrganizerId = organizerId,
                    TotalSeats = 100,
                    AvailableSeats = 85,
                    Price = 50.00m,
                   
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}