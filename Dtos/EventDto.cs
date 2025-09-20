namespace Event_booking.Api.Dtos
{
    public class EventDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalSeats { get; set; }
        public decimal Price { get; set; }

        // only Admin might set this explicitly
        public Guid OrganizerId { get; set; }
    }

}
