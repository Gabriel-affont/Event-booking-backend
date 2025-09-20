using Event_booking.Api.Dtos;
using Event_booking.Api.Models;
using EventBooking.Api.Data;
using EventBooking.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);
        }

        
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            return Ok(ev);
        }

        
        [Authorize(Roles = "Organizer")]
        [HttpGet("my-events")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var events = await _context.Events
                .Where(e => e.OrganizerId == Guid.Parse(userId))
                .ToListAsync();

            return Ok(events);
        }


        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromForm] EventDto dto, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var ev = new Event
            {
                id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                Date = dto.Date,
                TotalSeats = dto.TotalSeats,
                AvailableSeats = dto.TotalSeats,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow,
                OrganizerId = User.IsInRole("Organizer") ? Guid.Parse(userId) : dto.OrganizerId
            };

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowed.Contains(imageFile.ContentType))
                    return BadRequest("Invalid image format.");

                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // ✅ Ensure directory exists
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var savePath = Path.Combine(uploadFolder, fileName);

                // ✅ This is where FileStream is used
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                ev.ImageUrl = $"{baseUrl}/uploads/{fileName}";
            }


            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.id }, ev);
        }



        [Authorize(Roles = "Organizer,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromForm] EventDto dto, IFormFile? imageFile)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Organizer") && ev.OrganizerId != Guid.Parse(userId))
                return Forbid();

            // Update event properties
            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.Location = dto.Location;
            ev.Date = dto.Date;
            ev.TotalSeats = dto.TotalSeats;
            ev.Price = dto.Price;

            // Replace image if new one is uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowed.Contains(imageFile.ContentType))
                    return BadRequest("Invalid image format.");

                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}"; // unique filename
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!); // ensure folder exists

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                ev.ImageUrl = $"{baseUrl}/uploads/{fileName}";
            }

            await _context.SaveChangesAsync();

            // return updated event with new ImageUrl
            return Ok(ev);
        }




        [Authorize(Roles = "Organizer,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            if (User.IsInRole("Organizer") && ev.OrganizerId != Guid.Parse(userId))
                return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
