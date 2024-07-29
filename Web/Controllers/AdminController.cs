using Core.IRepo;
using Core.Models;
using Cores.Models.Shared;
using Data.Entities;
using Data.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminRepo _adminRepo;
        private readonly ILocationService _locationService;
        private readonly IRoleService _roleService;
        private readonly IMembersRepo _membersRepo;
        private readonly ISMSService _sMSService;
        private readonly UserManager<Persona> _userManager;
        public AdminController(IAdminRepo adminRepo, ILocationService locationService, IRoleService roleService, IMembersRepo membersRepo, ISMSService sMSService, UserManager<Persona> user)
        {
            _adminRepo = adminRepo;
            _locationService = locationService;
            _roleService = roleService;
            _membersRepo = membersRepo;
            _sMSService = sMSService;
            _userManager = user;

        }
        public async Task<IActionResult> Index([FromQuery] FilterOptions filter)
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId);
            var username = userInDb.UserName;
            if (username != "SuperAdmin")
            {
                var admin = await _adminRepo.GetPaginatedListAsync(filter, userId);
                ViewBag.Filter = filter;
                return View(admin);
            }
             var admins = await _adminRepo.GetPaginatedListAsync(filter);
             ViewBag.Filter = filter;

            return View(admins);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Create"}
            };
            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminViewModel model)
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var username = user?.UserName;
            var response = await _adminRepo.CreateAsync(model, username);
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var admin = await _adminRepo.GetByIdAsync(id);
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Edit"}
            };
            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");

            return View(admin);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminViewModel model)
        {
            var response = await _adminRepo.UpdateAsync(model, "Admin");
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _adminRepo.DeleteAsync(id, "Admin");
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var admin = await _adminRepo.GetByIdAsync(id);
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Details"}
            };

            return View(admin);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var employee = await _adminRepo.GetByUserIdAsync(userId);

            return View(employee);
        }

        public async Task<IActionResult> UpdateProfile(string UserName, string Email, string Password, Guid id)
        {
            var response = await _adminRepo.UpdateProfile(UserName, Email, Password, id);
            if (response.Status)
            {
                TempData["success"] = response.Message;
            }
            else
            {
                TempData["error"] = response.Message;
            }

            return RedirectToAction(nameof(Profile));
        }




        [HttpGet]
        public async Task<IActionResult> SendSms()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendReminder(string message)
        {
            var user = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var membersPhone = await _membersRepo.GetAllMembersPhoneNumber(user);
            var formattedPhones = membersPhone
                .Where(phone => !string.IsNullOrEmpty(phone)) // Ensure the phone number is not null or empty
                .Select(phone => Convert.ToInt64($"234{phone.Substring(1)}"))
                 .ToList();

            if (message is null)
            {
                string messages = "Salamu alykum warahamatullah, 011001002, FirstBank";

                var response = await _sMSService.SendSmS(formattedPhones, messages);

                TempData["BaseResponse"] = JsonSerializer.Serialize(response);

                return RedirectToAction(nameof(SendSms));

            }
            else
            {

                var response = await _sMSService.SendSmS(formattedPhones, message);

                TempData["BaseResponse"] = JsonSerializer.Serialize(response);

                return RedirectToAction(nameof(SendSms));
            }


        }



        #region endpoints

        [HttpGet]
        [Route("/api/v1/v2/getLga")]
        public async Task<IActionResult> GetLocalGovernments(Guid sId)
        {
            var data = await _locationService.GetLocalGovernmentsAsync(sId);
            List<LocalGovernmentViewModel>? lGViewModels = data.ToList();
            if (lGViewModels.Any())
            {
                return StatusCode(200, new
                {
                    Lgas = lGViewModels
                });
            }

            return StatusCode(400, new
            {
                message = "Local Governments not found"
            });
        }

        #endregion
    }

}
