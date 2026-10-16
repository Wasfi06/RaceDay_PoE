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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates or records a result for a participant enrollment.
        /// Organizer access required.
        /// </summary>
        [HttpPost]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> CreateResult(
            CreateResultRequest request)
        {
            var organizerId = (int)HttpContext.Items["UserId"]!;

            var enrollment = await _context.Enrollments
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == request.EnrollmentId);

            if (enrollment == null)
            {
                return NotFound(new
                {
                    message = "Enrollment not found."
                });
            }

            if (enrollment.Event.OrganizerId != organizerId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only record results for your own events."
                });
            }

            if (request.FinishPosition.HasValue &&
                request.FinishPosition.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Finish position must be greater than zero."
                });
            }

            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r =>
                    r.EnrollmentId == request.EnrollmentId);

            if (existingResult != null)
            {
                return BadRequest(new
                {
                    message = "A result already exists for this enrollment."
                });
            }

            var result = new Result
            {
                EnrollmentId = request.EnrollmentId,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition,
                IsPublished = request.IsPublished,
                PublishedAt = request.IsPublished
                    ? DateTime.UtcNow
                    : null
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultId },
                new
                {
                    message = "Result recorded successfully.",
                    resultId = result.ResultId
                });
        }

        /// <summary>
        /// Gets a result by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetResult(int id)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var role = HttpContext.Items["Role"] as string;

            var result = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            if (role == "Participant" &&
                 result.Enrollment.ParticipantId != userId)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            if (role == "Organizer" &&
                result.Enrollment.Event.OrganizerId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only view results for your own events."
                });
            }

            return Ok(new
            {
                result.ResultId,
                result.EnrollmentId,
                EventId = result.Enrollment.EventId,
                EventName = result.Enrollment.Event.Name,
                ParticipantId = result.Enrollment.ParticipantId,
                ParticipantName =
                    result.Enrollment.Participant.FirstName + " " +
                    result.Enrollment.Participant.LastName,
                CategoryId = result.Enrollment.CategoryId,
                CategoryName = result.Enrollment.Category.Name,
                result.FinishTime,
                result.FinishPosition,
                result.IsPublished,
                result.PublishedAt
            });
        }

        /// <summary>
        /// Gets all results belonging to the logged-in Participant.
        /// </summary>
        [HttpGet("my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantId = (int)HttpContext.Items["UserId"]!;

            var results = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrollment.ParticipantId == participantId)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrollmentId,
                    EventId = r.Enrollment.EventId,
                    EventName = r.Enrollment.Event.Name,
                    CategoryId = r.Enrollment.CategoryId,
                    CategoryName = r.Enrollment.Category.Name,
                    r.FinishTime,
                    r.FinishPosition,
                    r.IsPublished,
                    r.PublishedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Gets all results for an event owned by the logged-in Organizer.
        /// </summary>
        [HttpGet("event/{eventId:int}")]
        [SessionAuthorize("Organizer")]
        public async Task<IActionResult> GetEventResults(int eventId)
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
                    message = "You can only view results for your own events."
                });
            }

            var results = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrollment.EventId == eventId)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrollmentId,
                    ParticipantId = r.Enrollment.ParticipantId,
                    ParticipantName =
                        r.Enrollment.Participant.FirstName + " " +
                        r.Enrollment.Participant.LastName,
                    ParticipantEmail = r.Enrollment.Participant.Email,
                    CategoryId = r.Enrollment.CategoryId,
                    CategoryName = r.Enrollment.Category.Name,
                    r.FinishTime,
                    r.FinishPosition,
                    r.IsPublished,
                    r.PublishedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
