using EventBooking.Api.Models.Event_booking.Api.Models;

namespace Event_booking.Api.Models

{
   
    public class Booking
    {

        public int Id { get; set; }

        // Foreign keys
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties
        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;

        // Booking details
        public int NumberOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        
    }
}
