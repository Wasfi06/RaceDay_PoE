using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public AccountController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _apiService.CreateClient();

                var response = await client.PostAsJsonAsync(
                    "api/Auth/register", model);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Registration failed. Please check your details or try another email.");

                    return View(model);
                }

                TempData["Success"] =
                    "Your account has been created. Please log in.";

                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to contact RaceDay services. Please try again.");

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _apiService.CreateClient();

                var response = await client.PostAsJsonAsync(
                    "api/Auth/login", model);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Login failed. Please check your email and password.");

                    return View(model);
                }

                var result =
                    await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null ||
                    string.IsNullOrWhiteSpace(result.SessionId) ||
                    string.IsNullOrWhiteSpace(result.Role))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The server returned an invalid login response.");

                    return View(model);
                }

                var role = result.Role.Trim();

                if (role.Equals("Organizer",
                        StringComparison.OrdinalIgnoreCase))
                {
                    role = "Organizer";
                }
                else if (role.Equals("Participant",
                             StringComparison.OrdinalIgnoreCase))
                {
                    role = "Participant";
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The account has an unrecognised role.");

                    return View(model);
                }

                HttpContext.Session.Clear();

                HttpContext.Session.SetString(
                    "SessionId", result.SessionId);

                HttpContext.Session.SetInt32(
                    "UserId", result.UserId);

                HttpContext.Session.SetString(
                    "FirstName", result.FirstName);

                HttpContext.Session.SetString(
                    "LastName", result.LastName);

                HttpContext.Session.SetString(
                    "Email", result.Email);

                HttpContext.Session.SetString(
                    "Role", role);

                HttpContext.Session.SetString(
                    "SessionExpiresAt",
                    result.ExpiresAt.ToString("O"));

                if (role == "Organizer")
                {
                    return RedirectToAction(
                        "Dashboard", "Organizer");
                }

                return RedirectToAction(
                    "Index", "Participant");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to contact RaceDay services. Please try again.");

                return View(model);
            }
            catch (System.Text.Json.JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The server returned an unexpected response.");

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}
