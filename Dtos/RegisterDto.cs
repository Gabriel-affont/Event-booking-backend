using EventBooking.Api.Models;

namespace Event_booking.Api.Dtos
{
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Attendee; // add this
    }
}
