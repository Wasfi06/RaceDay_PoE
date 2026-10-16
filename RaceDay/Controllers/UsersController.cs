using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Middleware;
using RaceDay.Services;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobStorageService;

        public UsersController(
            RaceDayDbContext context,
            IBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// </summary>
        [HttpGet("me")]
        [SessionAuthorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = (int)HttpContext.Items["UserId"]!;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.Phone,
                user.ProfileImageUrl,
                user.CreatedAt,
                user.IsActive
            });
        }

        /// <summary>
        /// Updates the profile of the currently authenticated user.
        /// </summary>
        [HttpPut("me")]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateProfileRequest request)
        {
            var userId = (int)HttpContext.Items["UserId"]!;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.ProfileImageUrl = request.ProfileImageUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
        /// <summary>
        /// Uploads a profile picture for the currently authenticated user.
        /// </summary>
        [HttpPost("me/profile-picture")]
        [SessionAuthorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(
            IFormFile file)
        {
            const long maxFileSize = 5 * 1024 * 1024;

            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select an image to upload."
                });
            }

            if (file.Length > maxFileSize)
            {
                return BadRequest(new
                {
                    message = "The image must not exceed 5 MB."
                });
            }

            var allowedTypes = new[]
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedTypes.Contains(
                file.ContentType,
                StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only JPEG, PNG, and WebP images are allowed."
                });
            }

            var userId = (int)HttpContext.Items["UserId"]!;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            try
            {
                await using var stream = file.OpenReadStream();

                var imageUrl = await _blobStorageService.UploadProfileImageAsync(
                    stream,
                    file.FileName,
                    file.ContentType);

                user.ProfileImageUrl = imageUrl;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile picture uploaded successfully.",
                    profileImageUrl = imageUrl
                });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(503, new
                {
                    message = "Profile picture storage is not configured."
                });
            }
            catch (Azure.RequestFailedException)
            {
                return StatusCode(502, new
                {
                    message = "The image could not be saved to storage."
                });
            }
        }
    }
}
