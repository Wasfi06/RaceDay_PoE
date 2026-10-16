using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
//w
namespace RaceDayMVC.Controllers
{
    public class OrganizerController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public OrganizerController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsOrganizer()
        {
            return HttpContext.Session.GetString("Role") == "Organizer";
        }

        private string? GetSessionId()
        {
            return HttpContext.Session.GetString("SessionId");
        }

        public IActionResult Dashboard()
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events()
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _apiService.CreateClient();

                var events = await client.GetFromJsonAsync<List<EventViewModel>>(
                    "api/Events");

                int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;

                var myEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.OrganizerId == organizerId)
                    .ToList();

                return View(myEvents);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to contact the RaceDay API. Please try again.";

                return View(new List<EventViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _apiService.CreateClient();

                var sessionId = HttpContext.Session.GetString("SessionId");

                if (string.IsNullOrEmpty(sessionId))
                    return RedirectToAction("Login", "Account");

                client.DefaultRequestHeaders.Remove("X-Session-Id");
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var response = await client.DeleteAsync($"api/Events/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event deleted successfully.";
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    TempData["ErrorMessage"] =
                        $"Unable to delete event. {errorMessage}";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to contact the RaceDay API. Please try again.";
            }

            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
            public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();

                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var raceEvent = await client.GetFromJsonAsync<EventViewModel>(
                    $"api/Events/{id}");

                if (raceEvent == null)
                    return NotFound();

                // Only allow an organizer to view their own event.
                int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (raceEvent.OrganizerId != organizerId)
                    return Forbid();

                return View(raceEvent);
            }
            catch (HttpRequestException ex)
            {
                return Content(
                    $"API request failed. Status: {ex.StatusCode}. Details: {ex.Message}");
            }
        }

              [HttpGet]
                public async Task<IActionResult> EventEnrollments(int eventId)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var enrollments =
                    await client.GetFromJsonAsync<List<OrganizerEnrollmentViewModel>>(
                        $"api/Enrollments/event/{eventId}");

                ViewBag.EventId = eventId;

                return View("Enrollments",
                    enrollments ?? new List<OrganizerEnrollmentViewModel>());
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    503,
                    "Unable to contact the RaceDay API. Please try again.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EventCategories(int eventId)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var categories =
                    await client.GetFromJsonAsync<List<CategoryViewModel>>(
                        $"api/Categories/event/{eventId}");

                ViewBag.EventId = eventId;

                return View("Categories",
                    categories ?? new List<CategoryViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to contact the RaceDay API. Please try again.";

                return View("Categories", new List<CategoryViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateCategory(int eventId)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            return View(new CreateCategoryViewModel
            {
                EventId = eventId,
                CategoryType = "Age"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryViewModel model)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var request = new
                {
                    model.Name,
                    model.CategoryType,
                    model.MinAge,
                    model.MaxAge,
                    model.MinDistanceKm,
                    model.MaxDistanceKm
                };

                var response = await client.PostAsJsonAsync(
                    $"api/Categories/event/{model.EventId}",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category created successfully.";
                    return RedirectToAction(
                        nameof(EventCategories),
                        new { eventId = model.EventId });
                }

                var apiMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(
                    string.Empty,
                    $"The API could not create the category. {apiMessage}");

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to contact the RaceDay API. Please try again.");

                return View(model);
            }
        }


        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            return View(new CreateEventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
            CreateEventViewModel model)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var response = await client.PostAsJsonAsync("api/Events", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Your event was created successfully.";

                    return RedirectToAction(nameof(Events));
                }

                var errorBody = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"The API could not create the event (HTTP {(int)response.StatusCode}). {errorBody}");

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to contact the RaceDay API. Please try again.");

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EventResults(int eventId)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var raceEvent = await client.GetFromJsonAsync<EventViewModel>(
                    $"api/Events/{eventId}");

                if (raceEvent == null)
                    return NotFound();

                int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (raceEvent.OrganizerId != organizerId)
                    return Forbid();

                var enrollments =
                    await client.GetFromJsonAsync<List<OrganizerEnrollmentViewModel>>(
                        $"api/Enrollments/event/{eventId}")
                    ?? new List<OrganizerEnrollmentViewModel>();

                var results =
                    await client.GetFromJsonAsync<List<ResultViewModel>>(
                        $"api/Results/event/{eventId}")
                    ?? new List<ResultViewModel>();

                var resultsByEnrollment = results.ToDictionary(
                    result => result.EnrollmentId);

                var model = new OrganizerEventResultsViewModel
                {
                    EventId = raceEvent.EventId,
                    EventName = raceEvent.Name,
                    Participants = enrollments.Select(enrollment =>
                    {
                        resultsByEnrollment.TryGetValue(
                            enrollment.EnrollmentId, out var result);

                        return new OrganizerResultRowViewModel
                        {
                            EnrollmentId = enrollment.EnrollmentId,
                            ParticipantName = enrollment.ParticipantName,
                            ParticipantEmail = enrollment.ParticipantEmail,
                            CategoryName = enrollment.CategoryName,
                            ResultId = result?.ResultId,
                            FinishTime = result?.FinishTime,
                            FinishPosition = result?.FinishPosition,
                            IsPublished = result?.IsPublished ?? false
                        };
                    }).ToList()
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    503,
                    "Unable to load event results. Please check that the API is running.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(
            int eventId,
            int enrollmentId,
            string? finishTime,
            int? finishPosition,
            bool isPublished)
        {
            if (!IsOrganizer())
                return RedirectToAction("Login", "Account");

            var sessionId = GetSessionId();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            TimeSpan? parsedFinishTime = null;

            if (!string.IsNullOrWhiteSpace(finishTime))
            {
                if (!TimeSpan.TryParse(
                    finishTime,
                    CultureInfo.InvariantCulture,
                    out var parsed))
                {
                    TempData["ResultError"] =
                        "Enter the finish time in HH:MM:SS format, for example 00:52:30.";

                    return RedirectToAction(
                        nameof(EventResults), new { eventId });
                }

                if (parsed < TimeSpan.Zero)
                {
                    TempData["ResultError"] = "Finish time cannot be negative.";

                    return RedirectToAction(
                        nameof(EventResults), new { eventId });
                }

                parsedFinishTime = parsed;
            }

            if (finishPosition.HasValue && finishPosition.Value <= 0)
            {
                TempData["ResultError"] =
                    "Finish position must be greater than zero.";

                return RedirectToAction(
                    nameof(EventResults), new { eventId });
            }

            if (!parsedFinishTime.HasValue && !finishPosition.HasValue)
            {
                TempData["ResultError"] =
                    "Enter a finish time, a finish position, or both.";

                return RedirectToAction(
                    nameof(EventResults), new { eventId });
            }

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Add("X-Session-Id", sessionId);

                var request = new
                {
                    EnrollmentId = enrollmentId,
                    FinishTime = parsedFinishTime,
                    FinishPosition = finishPosition,
                    IsPublished = isPublished
                };

                var response = await client.PostAsJsonAsync(
                    "api/Results", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["ResultSuccess"] = isPublished
                        ? "Result recorded and published successfully."
                        : "Result recorded successfully. It is not yet published.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["ResultError"] =
                        $"Could not record the result (HTTP {(int)response.StatusCode}). {error}";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ResultError"] =
                    "Unable to contact the RaceDay API.";
            }

            return RedirectToAction(nameof(EventResults), new { eventId });
        }

    }
}
