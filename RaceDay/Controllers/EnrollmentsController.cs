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
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrollmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates an enrollment for the logged-in Participant.
        /// </summary>
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> CreateEnrollment(
            CreateEnrollmentRequest request)
        {
            var participantId = (int)HttpContext.Items["UserId"]!;

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == request.EventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            if (category.EventId != request.EventId)
            {
                return BadRequest(new
                {
                    message = "The selected category does not belong to this event."
                });
            }

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e =>
                    e.EventId == request.EventId &&
                    e.ParticipantId == participantId);

            if (alreadyEnrolled)
            {
                return BadRequest(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            var enrollment = new Enrollment
            {
                EventId = request.EventId,
                ParticipantId = participantId,
                CategoryId = request.CategoryId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMyEnrollment),
                new { id = enrollment.EnrollmentId },
                new
                {
                    message = "Enrollment created successfully.",
                    enrollmentId = enrollment.EnrollmentId
                });
        }

        /// <summary>
        /// Gets a single enrollment belonging to the logged-in Participant.
        /// </summary>
        [HttpGet("my/{id:int}")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrollment(int id)
        {
            var participantId = (int)HttpContext.Items["UserId"]!;

            var enrollment = await _context.Enrollments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == id &&
                    e.ParticipantId == participantId);

            if (enrollment == null)
            {
                return NotFound(new
                {
                    message = "Enrollment not found."
                });
            }

            return Ok(new
            {
                enrollment.EnrollmentId,
                enrollment.EventId,
                EventName = enrollment.Event.Name,
                enrollment.CategoryId,
                CategoryName = enrollment.Category.Name,
                enrollment.EnrollmentDate,
                enrollment.Status
            });
        }

        /// <summary>
        /// Gets all enrollments belonging to the logged-in Participant.
        /// </summary>
        [HttpGet("my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var participantId = (int)HttpContext.Items["UserId"]!;

            var enrollments = await _context.Enrollments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantId == participantId)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.EventId,
                    EventName = e.Event.Name,
                    e.CategoryId,
                    CategoryName = e.Category.Name,
                    e.EnrollmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        /// <summary>
        /// Gets all enrollments for an event owned by the logged-in Organizer.
        /// </summary>
        [HttpGet("event/{eventId:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> GetEventEnrollments(int eventId)
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
                    message = "You can only view enrollments for your own events."
                });
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.ParticipantId,
                    ParticipantName = e.Participant.FirstName + " " +
                                     e.Participant.LastName,
                    ParticipantEmail = e.Participant.Email,
                    e.CategoryId,
                    CategoryName = e.Category.Name,
                    e.EnrollmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrollments);
        }
    }
}
