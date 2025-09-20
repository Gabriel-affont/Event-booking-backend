namespace Event_booking.Api.Dto
{
    public class BookingDto
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOfSeats { get; set; }
    }
}
