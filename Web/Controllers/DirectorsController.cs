using Core.Constants;
using Core.IRepo;
using Core.Models;
using Cores.Models.Shared;
using Data;
using Data.Entities;
using Data.Repo;
using Data.Services.Implementation;
using Data.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Controllers
{
    public class DirectorsController : Controller
    {
        private readonly IDirectorRepo _directorRepo;
        private readonly ILocationService _locationService;
        private readonly IRoleService _roleService;
        private readonly UserManager<Persona> _userManager;
        private readonly AdtDbContext _context;
        private readonly IAdminRepo _adminRepo;
        private readonly IMembersRepo _membersRepo;
        private readonly ISMSService _smssService;
        public DirectorsController(IDirectorRepo directorRepo, ILocationService locationService,
            IRoleService roleService, UserManager<Persona> userManager, AdtDbContext adtDbContext, 
            IAdminRepo adminRepo, IMembersRepo membersRepo, ISMSService sMSService)
        {
            _directorRepo = directorRepo;
            _locationService = locationService;
            _roleService = roleService;
            _userManager = userManager;
            _context = adtDbContext;
            _adminRepo = adminRepo;
            _membersRepo = membersRepo;
            _smssService = sMSService;

        }
        public async Task<IActionResult> Index([FromQuery] FilterOptions filter)
        {
            var directors = await _directorRepo.GetPaginatedListAsync(filter);
            ViewBag.Filter = filter;

            return View(directors);
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
        public async Task<IActionResult> Create(DirectorViewModel model)
        {
            var response = await _directorRepo.CreateAsync(model, "Admin");
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
            var director = await _directorRepo.GetByIdAsync(id);
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Edit"}
            };
            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");

            return View(director);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DirectorViewModel model)
        {
            var response = await _directorRepo.UpdateAsync(model, "Admin");
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");
            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _directorRepo.DeleteAsync(id, "Admin");
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
            var director = await _directorRepo.GetByIdAsync(id);
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Details"}
            };

            return View(director);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var director = await _directorRepo.GetByUserIdAsync(userId);

            return View(director);
        }

        public async Task<IActionResult> UpdateProfile(string UserName, string Email, string Password, Guid id)
        {
            var response = await _directorRepo.UpdateProfile(UserName, Email, Password, id);
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
        public async Task<ActionResult> SendSmS()
        {
            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");

            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId);
            var username = userInDb.UserName;
            var directorInDb = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == userInDb.Id);

            if (username != "SuperAdmin")
            {
                ViewData["stateId"] = directorInDb.StateId;
                ViewData["lgas"] = new SelectList(await _locationService.GetLocalGovernmentsAsync(directorInDb.StateId), "Id", "Name");
                return View();
            }

            return View();
        }


        [HttpPost]
        public async Task<ActionResult> SendSMS(SMSViewModel model)
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            IEnumerable<string> phoneNumbers = new List<string>();

            if (User.IsInRole(RoleConstants.SuperAdmin))
            {
                ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");

                if (model.StateId is not null && model.LocalGovernmentId is not null )
                {
                    phoneNumbers = await _locationService.GetPhoneNumbersByStateAndLGAAsync(model.StateId.Value, model.LocalGovernmentId.Value, model.RecipientType);
                }
                else if (model.StateId is not null && model.LocalGovernmentId is null)
                {
                    phoneNumbers = await _locationService.GetPhoneNumbersByStateAndLGAAsync(model.StateId.Value, model.RecipientType);
                }
                else if (model.StateId is null && model.LocalGovernmentId is null)
                {
                    phoneNumbers = await _locationService.GetAllPhoneNumbersAsync();
                }
            }
            if (User.IsInRole(RoleConstants.Director))
            {
                var directorInDb = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == userId);
                ViewData["stateId"] = directorInDb.StateId;
                ViewData["lgas"] = new SelectList(await _locationService.GetLocalGovernmentsAsync(directorInDb.StateId), "Id", "Name");

                if (model.StateId is not null && model.LocalGovernmentId is not null)
                {
                    phoneNumbers = await _locationService.GetPhoneNumbersByStateAndLGAAsync(model.StateId.Value, model.LocalGovernmentId.Value, model.RecipientType);
                }
                else if (model.StateId is not null && model.LocalGovernmentId is null)
                {
                    phoneNumbers = await _locationService.GetPhoneNumbersByStateAndLGAAsync(model.StateId.Value, model.RecipientType);
                }
            }
            var formattedPhones = phoneNumbers
               .Where(phone => !string.IsNullOrEmpty(phone)) // Ensure the phone number is not null or empty
               .Select(phone => Convert.ToInt64($"234{phone.Substring(1)}"))
                .ToList();

            // Add your logic to send SMS using the phone numbers
            var response = await _smssService.SendSmS(formattedPhones, model.Message);

            TempData["BaseResponse"] = JsonSerializer.Serialize(response);

            return View();
        }




    }
}
