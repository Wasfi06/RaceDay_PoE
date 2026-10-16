using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Middleware;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all categories for an event.
        /// </summary>
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.CategoryType,
                    c.MinAge,
                    c.MaxAge,
                    c.MinDistanceKm,
                    c.MaxDistanceKm
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Gets a category by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(new
            {
                category.CategoryId,
                category.EventId,
                EventName = category.Event.Name,
                category.Name,
                category.CategoryType,
                category.MinAge,
                category.MaxAge,
                category.MinDistanceKm,
                category.MaxDistanceKm
            });
        }

        /// <summary>
        /// Creates a category for an event. Organizer access required.
        /// </summary>
        [HttpPost("event/{eventId:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            CreateCategoryRequest request)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (eventItem.OrganizerId != organizerId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only manage categories for your own events."
                });
            }

            if (request.CategoryType != "Age" &&
                request.CategoryType != "Distance")
            {
                return BadRequest(new
                {
                    message = "CategoryType must be Age or Distance."
                });
            }

            if (request.CategoryType == "Age")
            {
                if (!request.MinAge.HasValue ||
                    !request.MaxAge.HasValue ||
                    request.MinAge < 0 ||
                    request.MaxAge < request.MinAge)
                {
                    return BadRequest(new
                    {
                        message = "Age categories require valid MinAge and MaxAge values."
                    });
                }
            }

            if (request.CategoryType == "Distance")
            {
                if (!request.MinDistanceKm.HasValue ||
                    !request.MaxDistanceKm.HasValue ||
                    request.MinDistanceKm < 0 ||
                    request.MaxDistanceKm < request.MinDistanceKm)
                {
                    return BadRequest(new
                    {
                        message = "Distance categories require valid MinDistanceKm and MaxDistanceKm values."
                    });
                }
            }

            var duplicate = await _context.Categories
                .AnyAsync(c =>
                    c.EventId == eventId &&
                    c.Name == request.Name);

            if (duplicate)
            {
                return BadRequest(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            var category = new Category
            {
                EventId = eventId,
                Name = request.Name,
                CategoryType = request.CategoryType,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                MinDistanceKm = request.MinDistanceKm,
                MaxDistanceKm = request.MaxDistanceKm,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                new
                {
                    message = "Category created successfully.",
                    categoryId = category.CategoryId
                });
        }

        /// <summary>
        /// Updates a category. Organizer access required.
        /// </summary>
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            UpdateCategoryRequest request)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            if (category.Event.OrganizerId != organizerId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only manage categories for your own events."
                });
            }

            if (request.CategoryType != "Age" &&
                request.CategoryType != "Distance")
            {
                return BadRequest(new
                {
                    message = "CategoryType must be Age or Distance."
                });
            }

            if (request.CategoryType == "Age")
            {
                if (!request.MinAge.HasValue ||
                    !request.MaxAge.HasValue ||
                    request.MinAge < 0 ||
                    request.MaxAge < request.MinAge)
                {
                    return BadRequest(new
                    {
                        message = "Age categories require valid MinAge and MaxAge values."
                    });
                }
            }

            if (request.CategoryType == "Distance")
            {
                if (!request.MinDistanceKm.HasValue ||
                    !request.MaxDistanceKm.HasValue ||
                    request.MinDistanceKm < 0 ||
                    request.MaxDistanceKm < request.MinDistanceKm)
                {
                    return BadRequest(new
                    {
                        message = "Distance categories require valid MinDistanceKm and MaxDistanceKm values."
                    });
                }
            }

            var duplicate = await _context.Categories
                .AnyAsync(c =>
                    c.EventId == category.EventId &&
                    c.Name == request.Name &&
                    c.CategoryId != id);

            if (duplicate)
            {
                return BadRequest(new
                {
                    message = "A category with this name already exists for this event."
                });
            }

            category.Name = request.Name;
            category.CategoryType = request.CategoryType;
            category.MinAge = request.MinAge;
            category.MaxAge = request.MaxAge;
            category.MinDistanceKm = request.MinDistanceKm;
            category.MaxDistanceKm = request.MaxDistanceKm;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        /// <summary>
        /// Deletes a category. Organizer access required.
        /// </summary>
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            if (category.Event.OrganizerId != organizerId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only manage categories for your own events."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }
}
