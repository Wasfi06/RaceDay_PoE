using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Diagnostics;
//ssdadaadas
namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ApiTest(
    [FromServices] RaceDayMVC.Services.RaceDayApiService apiService)
        {
            try
            {
                var client = apiService.CreateClient();

                var response = await client.GetAsync("api/Events");

                var body = await response.Content.ReadAsStringAsync();

                ViewBag.StatusCode = (int)response.StatusCode;
                ViewBag.ResponseBody = body;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

    }
}
