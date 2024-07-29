using Core.Constants;
using Data.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILocationService _locationService;

        public HomeController(ILogger<HomeController> logger, ILocationService locationService)
        {
            _logger = logger;
            _locationService = locationService;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(RoleConstants.SuperAdmin))
                    return RedirectToAction(nameof(Dashboard));
                if (User.IsInRole(RoleConstants.Director))
                    return RedirectToAction(nameof(AdminController.Profile), "Directors");
                if (User.IsInRole(RoleConstants.Coordinator))
                    return RedirectToAction(nameof(AdminController.Profile), "Admin");
            }
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            var model = await _locationService.GetDashboardReportAsync();

            return View(model);
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
    }
}
