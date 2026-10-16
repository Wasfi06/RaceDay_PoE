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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all events.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.EventType,
                    e.RouteUrl,
                    e.RouteDescription,
                    e.BannerImageUrl,
                    e.OrganizerId,
                    OrganizerName = e.Organizer.FirstName + " " + e.Organizer.LastName
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Gets a single event by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(new
            {
                eventItem.EventId,
                eventItem.Name,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.DistanceKm,
                eventItem.EventType,
                eventItem.RouteUrl,
                eventItem.RouteDescription,
                eventItem.BannerImageUrl,
                eventItem.OrganizerId,
                OrganizerName = eventItem.Organizer.FirstName + " " + eventItem.Organizer.LastName,
                Categories = eventItem.Categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.CategoryType,
                    c.MinAge,
                    c.MaxAge,
                    c.MinDistanceKm,
                    c.MaxDistanceKm
                })
            });
        }

        /// <summary>
        /// Creates a new event. Organizer access required.
        /// </summary>
        [HttpPost]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> CreateEvent(CreateEventRequest request)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var eventItem = new Event
            {
                OrganizerId = organizerId,
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                RouteUrl = request.RouteUrl,
                RouteDescription = request.RouteDescription,
                BannerImageUrl = request.BannerImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventId },
                new
                {
                    message = "Event created successfully.",
                    eventId = eventItem.EventId
                });
        }

        /// <summary>
        /// Updates an event. Only the Organizer who created the event can update it.
        /// </summary>
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            UpdateEventRequest request)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

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
                    message = "You can only update your own events."
                });
            }

            eventItem.Name = request.Name;
            eventItem.Description = request.Description;
            eventItem.EventDate = request.EventDate;
            eventItem.Location = request.Location;
            eventItem.DistanceKm = request.DistanceKm;
            eventItem.EventType = request.EventType;
            eventItem.RouteUrl = request.RouteUrl;
            eventItem.RouteDescription = request.RouteDescription;
            eventItem.BannerImageUrl = request.BannerImageUrl;
            eventItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully."
            });
        }

        /// <summary>
        /// Deletes an event. Only the Organizer who created the event can delete it.
        /// </summary>
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

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
                    message = "You can only delete your own events."
                });
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }
}
