
using EventBooking.Api.Models.Event_booking.Api.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_booking.Api.Models
{
    public class Event
    {
        public Guid id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? ImageUrl { get; set; }
        public Guid OrganizerId { get; set; }
        //Foreign key User
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("OrganizerId")]
        public User? Organizer { get; set; }



    }
}
