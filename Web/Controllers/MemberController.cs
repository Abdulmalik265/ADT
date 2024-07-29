using Core.IRepo;
using Core.Models;
using Cores.Models.Shared;
using Data;
using Data.Entities;
using Data.Repo;
using Data.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Web.Controllers
{
    public class MemberController : Controller
    {
        private readonly IMembersRepo _membersRepo;
        private readonly ILocationService _locationService;
        private readonly AdtDbContext _context;
        private readonly UserManager<Persona> _userManager;
        public MemberController(IMembersRepo membersRepo, ILocationService locationService, AdtDbContext adtDbContext, UserManager<Persona> userManager)
        {
            _membersRepo = membersRepo;
            _locationService = locationService;
            _context = adtDbContext;
            _userManager = userManager;
            
        }
        public async Task<IActionResult> Index([FromQuery] FilterOptions filter)
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            var member = await _membersRepo.GetPaginatedListAsync(filter, userId);
            ViewBag.filter = filter;

            return View(member);
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
        public async Task<IActionResult> Create(MembersViewModel model)
        {

            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var username = user?.UserName;
            var response = await _membersRepo.CreateAsync(model, username);

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
            var member = await _membersRepo.GetByIdAsync(id);
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Edit"}
            };
            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");

            return View(member);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MembersViewModel model)
        {
            var response = await _membersRepo.UpdateAsync(model, "Admin");
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
            var response = await _membersRepo.DeleteAsync(id, "Admin");
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
            var member = await _membersRepo.GetByIdAsync(id);
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Details"}
            };
            return View(member);
        }
    }
}
