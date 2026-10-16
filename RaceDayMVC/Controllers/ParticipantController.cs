using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ParticipantController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FirstName =
                HttpContext.Session.GetString("FirstName");

            try
            {
                var client = _apiService.CreateClient();

                var events = await client
                    .GetFromJsonAsync<List<EventViewModel>>(
                        "api/Events");

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "We couldn't load events. Please try again later.";

                return View(new List<EventViewModel>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();

                var raceEvent = await client
                    .GetFromJsonAsync<EventViewModel>($"api/Events/{id}");

                if (raceEvent == null)
                {
                    return NotFound();
                }

                var categories = await client
                    .GetFromJsonAsync<List<CategoryViewModel>>(
                        $"api/Categories/event/{id}");

                var viewModel = new EventDetailsViewModel
                {
                    Event = raceEvent,
                    Categories = categories ?? new List<CategoryViewModel>()
                };

                return View(viewModel);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503,
                    "Unable to retrieve event details. Please try again later.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollmentViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.EventId <= 0 || model.CategoryId <= 0)
            {
                TempData["EnrollmentError"] =
                    "Please select a valid event and category.";

                return RedirectToAction("Details",
                    new { id = model.EventId });
            }

            try
            {
                var client = _apiService.CreateClient();

                var sessionId = HttpContext.Session.GetString("SessionId");

                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    TempData["EnrollmentError"] =
                        "Your session has expired. Please log in again.";

                    return RedirectToAction("Login", "Account");
                }

                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var response = await client.PostAsJsonAsync(
                    "api/Enrollments", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content
                        .ReadFromJsonAsync<EnrollmentResponse>();

                    TempData["EnrollmentSuccess"] =
                        result?.Message ?? "Enrollment created successfully.";

                    TempData["EnrollmentId"] =
                        result?.EnrollmentId.ToString();

                    return RedirectToAction("Details",
                        new { id = model.EventId });
                }

                var errorDetails = await response.Content.ReadAsStringAsync();

                TempData["EnrollmentError"] =
                    $"Enrollment failed ({(int)response.StatusCode}): {errorDetails}";
            }
            catch (HttpRequestException)
            {
                TempData["EnrollmentError"] =
                    "Unable to contact the RaceDay API. Please try again later.";
            }

            return RedirectToAction("Details",
                new { id = model.EventId });
        }
        [HttpGet]
        public async Task<IActionResult> MyEnrollments()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();

                client.DefaultRequestHeaders.Add(
                    "X-Session-Id", sessionId);

                var enrollments = await client.GetFromJsonAsync<
                    List<MyEnrollmentViewModel>>("api/Enrollments/my");

                return View(enrollments ?? new List<MyEnrollmentViewModel>());
            }
            catch (HttpRequestException)
            {
                return StatusCode(503,
                    "Unable to retrieve your enrollments. Please try again later.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();

                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var response = await client.GetAsync("api/Users/me");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage =
                        "Unable to load your profile. Please try again.";

                    return View(new ProfileViewModel
                    {
                        FirstName = HttpContext.Session.GetString("FirstName")
                            ?? string.Empty
                    });
                }

                var profile = await response.Content
                    .ReadFromJsonAsync<ProfileViewModel>();

                if (profile == null)
                {
                    return StatusCode(502,
                        "The API returned an invalid profile response.");
                }

                return View(profile);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to contact the RaceDay API. Please try again later.";

                return View(new ProfileViewModel
                {
                    FirstName = HttpContext.Session.GetString("FirstName")
                        ?? string.Empty
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return RedirectToAction("Login", "Account");
            }

            // The API identifies the current user from the session.
            // Do not accept a user ID or role change from the form.
            ModelState.Remove(nameof(ProfileViewModel.UserId));
            ModelState.Remove(nameof(ProfileViewModel.ProfilePictureUrl));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _apiService.CreateClient();

                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var response = await client.PutAsJsonAsync(
                    "api/Users/me",
                    new
                    {
                        model.FirstName,
                        model.LastName,
                        model.Email,
                        model.Phone
                    });

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString(
                        "FirstName",
                        model.FirstName);

                    TempData["ProfileSuccess"] =
                        "Your profile has been updated successfully.";

                    return RedirectToAction(nameof(Profile));
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                ModelState.AddModelError(
                    string.Empty,
                    "The profile could not be updated. Please check your details and try again.");

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to contact the RaceDay API. Please try again later.");

                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(ProfileViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.ProfilePicture == null ||
                model.ProfilePicture.Length == 0)
            {
                TempData["ProfileError"] = "Please select an image to upload.";
                return RedirectToAction(nameof(Profile));
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (model.ProfilePicture.Length > maxFileSize)
            {
                TempData["ProfileError"] = "The image must be 5 MB or smaller.";
                return RedirectToAction(nameof(Profile));
            }

            var allowedTypes = new[]
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedTypes.Contains(model.ProfilePicture.ContentType))
            {
                TempData["ProfileError"] =
                    "Please select a JPEG, PNG, or WebP image.";

                return RedirectToAction(nameof(Profile));
            }

            try
            {
                using var client = _apiService.CreateClient();

                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                using var content = new MultipartFormDataContent();

                using var fileContent = new StreamContent(
                    model.ProfilePicture.OpenReadStream());

                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        model.ProfilePicture.ContentType);

                content.Add(
                    fileContent,
                    "file",
                    model.ProfilePicture.FileName);

                var response = await client.PostAsync(
                    "api/Users/me/profile-picture",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["ProfileSuccess"] =
                        "Your profile picture has been uploaded successfully.";

                    return RedirectToAction(nameof(Profile));
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                TempData["ProfileError"] =
                    $"Picture upload failed ({(int)response.StatusCode}). " +
                    "Please check the API configuration and try again.";

                return RedirectToAction(nameof(Profile));
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to contact the RaceDay API. Please try again later.";

                return RedirectToAction(nameof(Profile));
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Account");

            var sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var results = await client.GetFromJsonAsync<List<ResultViewModel>>(
                    "api/Results/my") ?? new List<ResultViewModel>();

                // The API currently returns unpublished results too,
                // so only show published results on the participant page.
                var publishedResults = results
                    .Where(result => result.IsPublished)
                    .OrderBy(result => result.FinishPosition ?? int.MaxValue)
                    .ToList();

                return View(publishedResults);
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    503,
                    "Unable to load your results. Please try again later.");
            }
        }

    }
}
