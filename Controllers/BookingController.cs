using Event_booking.Api.Dto;
using Event_booking.Api.Models;
using EventBooking.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Event_booking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BookingController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.User)
                .ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return NotFound();
            return booking;
        }
        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.UserId == Guid.Parse(userId)) 
                .Select(b => new
                {
                    b.Id,
                    b.NumberOfSeats,
                    b.TotalPrice,
                   
                    Event = new
                    {
                        b.Event.Title,
                        b.Event.Location,
                        b.Event.Date
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }


        [Authorize(Roles = "Attendee")]
        [HttpPost]

        public async Task<ActionResult<Booking>> CreateBooking(BookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null) return NotFound("Event not found.");

            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null) return NotFound("User not found.");

            if (dto.NumberOfSeats <= 0)
                return BadRequest("Number of seats must be greater than 0.");

            if (ev.AvailableSeats < dto.NumberOfSeats)
                return BadRequest("Not enough seats available.");

            // Calculate total price
            var totalPrice = dto.NumberOfSeats * ev.Price;

            // Create booking
            var booking = new Booking
            {
                EventId = ev.id,
                UserId = Guid.Parse(userId),
                NumberOfSeats = dto.NumberOfSeats,
                TotalPrice = totalPrice,
                BookingDate = DateTime.UtcNow
            };

            // Update event seat availability
            ev.AvailableSeats -= dto.NumberOfSeats;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBooking(Guid id, Booking updatedBooking)
        {
            var existingBooking = _context.Bookings.Find(id);
            if (existingBooking == null)
            {
                return NotFound();
            }
            existingBooking.UserId = updatedBooking.UserId;
            existingBooking.EventId = updatedBooking.EventId;
            existingBooking.NumberOfSeats = updatedBooking.NumberOfSeats;
            existingBooking.TotalPrice = updatedBooking.TotalPrice;
            _context.SaveChanges();
            return NoContent();
        }
        [Authorize] 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Event) // load related event
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == Guid.Parse(userId));

            if (booking == null)
                return NotFound("Booking not found or you don't have permission to cancel it.");

            
            booking.Event.AvailableSeats += booking.NumberOfSeats;

            
            var updatedEvent = booking.Event;

           
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

           
            return Ok(new
            {
                message = "Booking canceled successfully",
                eventId = updatedEvent.id,
                availableSeats = updatedEvent.AvailableSeats
            });
        }
    }

    }
