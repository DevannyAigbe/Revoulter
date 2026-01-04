using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Revoulter.Core.Models;

namespace Revoulter.Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet("health")]

        public IActionResult Health()
        {
            return Ok(new
            {
                status = "ok",
                message = "Privy Auth Backend is running",
                timestamp = DateTime.UtcNow
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
