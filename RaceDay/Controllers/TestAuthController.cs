using Microsoft.AspNetCore.Mvc;
using RaceDay.Middleware;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/test-auth")]
    public class TestAuthController : ControllerBase
    {
        /// <summary>
        /// Tests whether the current user has a valid session.
        /// </summary>
        [HttpGet("authenticated")]
        [SessionAuthorize]
        public IActionResult Authenticated()
        {
            return Ok(new
            {
                message = "You are authenticated.",
                userId = HttpContext.Items["UserId"],
                role = HttpContext.Items["Role"]
            });
        }

        /// <summary>
        /// Tests Organizer-only access.
        /// </summary>
        [HttpGet("organizer")]
        [SessionAuthorize("Organizer")]
        public IActionResult OrganizerOnly()
        {
            return Ok(new
            {
                message = "You have Organizer access.",
                userId = HttpContext.Items["UserId"],
                role = HttpContext.Items["Role"]
            });
        }

        /// <summary>
        /// Tests Participant-only access.
        /// </summary>
        [HttpGet("participant")]
        [SessionAuthorize("Participant")]
        public IActionResult ParticipantOnly()
        {
            return Ok(new
            {
                message = "You have Participant access.",
                userId = HttpContext.Items["UserId"],
                role = HttpContext.Items["Role"]
            });
        }
    }
}