using Event_booking.Api.Models;
using EventBooking.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Event_booking.Api.Controllers
{
   
        [ApiController]
        [Route("api/[controller]")]
        public class CategoriesController : ControllerBase
        {
            private readonly AppDbContext _context;
            public CategoriesController(AppDbContext context)
            {
                _context = context;
            }
            [HttpGet]
            public IActionResult GetCategories()
            {
                var results = _context.Categories.ToList();
                return Ok(results);
            }
            [HttpGet("{id}")]
            public IActionResult GetCategory(int id)
            {
                var result = _context.Categories.Find(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            [HttpPost]
            public IActionResult CreateCategory(Category newCategory)
            {
                _context.Categories.Add(newCategory);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, newCategory);
            }
            [HttpPut("{id}")]
            public IActionResult UpdateCategory(int id, Category updatedCategory)
            {
                var existingCategory = _context.Categories.Find(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }
                existingCategory.Name = updatedCategory.Name;
                _context.SaveChanges();
                return NoContent();
            }
            [HttpDelete("{id}")]
            public IActionResult DeleteCategory(int id)
            {
                var existingCategory = _context.Categories.Find(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }
                _context.Categories.Remove(existingCategory);
                _context.SaveChanges();
                return NoContent();
            }
        }
}
