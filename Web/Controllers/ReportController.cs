using Core.Constants;
using Core.IRepo;
using Core.Models;
using Data;
using Data.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly IPdfService _pdfService;
        private readonly IMembersRepo _membersRepo;
        private readonly AdtDbContext _context;
        private readonly IDirectorRepo _directorRepo;
        private readonly IAdminRepo _adminRepo;

        public ReportController(ILocationService locationService, IPdfService dfService, IMembersRepo membersRepo, AdtDbContext adtDbContext,IDirectorRepo directorRepo, IAdminRepo adminRepo)
        {
            _locationService = locationService;
            _pdfService = dfService;
            _membersRepo = membersRepo;
            _context = adtDbContext;
            _directorRepo = directorRepo;
            _adminRepo = adminRepo;

        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAttendance()
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var directorInDb = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == userId);
            var adminInDb = await _context.Admins.SingleOrDefaultAsync(x => x.UserId == userId);
            if (User.IsInRole(RoleConstants.Director))
            {
                ViewData["stateId"] = directorInDb.StateId;
                ViewData["lgas"] = new SelectList(await _locationService.GetLocalGovernmentsAsync(directorInDb.StateId), "Id", "Name");

            }
            if (User.IsInRole(RoleConstants.Coordinator))
            {
                ViewData["adminlgaID"] = adminInDb.LocalGovernmentId;

            }

            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAttendance(AttendenceReportViewModel model)
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            ViewData["states"] = new SelectList(await _locationService.GetStatesAsync(), "Id", "Name");
            var membersAttendance = await _membersRepo.filterMembers(model);
            if (membersAttendance.Any())
            {

                var filter = "";
                if (model.StateId != null)
                    filter += $"State : {membersAttendance.First().LocalGovernment.State.Name} -";
                if (model.LgaId != null)
                    filter += $"Local Government : {membersAttendance.First().LocalGovernment.Name}";
                var stream = new MemoryStream();


                _pdfService.GenerateAttendenceSheet(stream, membersAttendance, filter);
                return File(stream.ToArray(), "application/pdf", $"AttendenceSheet{filter}.pdf");
            }
            var directorInDb = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == userId);
            var adminInDb = await _context.Admins.SingleOrDefaultAsync(x => x.UserId == userId);
            if (User.IsInRole(RoleConstants.Director))
            {
                ViewData["stateId"] = directorInDb.StateId;
                ViewData["lgas"] = new SelectList(await _locationService.GetLocalGovernmentsAsync(directorInDb.StateId), "Id", "Name");

            }
            if (User.IsInRole(RoleConstants.Coordinator))
            {
                ViewData["adminlgaID"] = adminInDb.LocalGovernmentId;

            }
            TempData["Error"] = "Couldn't match any member with the specified filter!";
            return View();

        }
        [HttpGet]
        public async Task<IActionResult> GenerateMembersReport()
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var adminInDb = await _context.Admins.SingleOrDefaultAsync( x => x.UserId == userId);
            ViewData["lgaId"] = adminInDb.LocalGovernmentId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMembersReport(AttendenceReportViewModel model)
        {

            var filterMembers = await _membersRepo.filterMembersPayment(model);
                var stream = new MemoryStream();

            if (filterMembers.Any())
            {
                _pdfService.GenerateMembersReport(stream, filterMembers, model);

                return File(stream.ToArray(), "application/pdf", $"PaymentReport_{filterMembers.First().LgName}.pdf");

            }
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var adminInDb = await _context.Admins.SingleOrDefaultAsync(x => x.UserId == userId);
            ViewData["lgaId"] = adminInDb.LocalGovernmentId;

            TempData["Error"] = "Couldn't match any member with the specified filter!";
            return View();
        }

        [HttpGet]
        public IActionResult GenerateDirectorsReport()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateDirectorsReport(AttendenceReportViewModel model)
        {
            var filterMembers = await _directorRepo.FilterDirectorsPayment(model);
            var stream = new MemoryStream();

            if (filterMembers.Any())
            {
                _pdfService.GenerateDirectorsReport(stream, filterMembers, model);

                return File(stream.ToArray(), "application/pdf", $"PaymentReport.pdf");

            }
            TempData["Error"] = "Couldn't match any member with the specified filter!";

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GenerateAdminsReport()
        {
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var directorInDB = await _context.Directors.SingleOrDefaultAsync(x=> x.UserId  == userId );
            ViewData["stateId"] = directorInDB.StateId;
            return View();
        } 
        
        [HttpPost]
        public async Task<IActionResult> GenerateAdminsReport(AttendenceReportViewModel model)
        {
            var filterMembers = await _adminRepo.filterAdminsPayment(model);
            var stream = new MemoryStream();

            if (filterMembers.Any())
            {
                _pdfService.GenerateAdminsReport(stream, filterMembers, model);

                return File(stream.ToArray(), "application/pdf", $"PaymentReport.pdf");

            }
            var userId = Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var directorInDB = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == userId);
            ViewData["stateId"] = directorInDB.StateId;
            TempData["Error"] = "Couldn't match any member with the specified filter!";

            return View();
        }
    }
}
