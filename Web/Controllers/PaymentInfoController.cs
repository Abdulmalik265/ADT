using Core.Enum;
using Core.IRepo;
using Core.Models;
using Cores.Models.Shared;
using Data.Repo;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class PaymentInfoController : Controller
    {
        private readonly IPaymentInfo _paymentInfo;
        public PaymentInfoController(IPaymentInfo paymentInfo)
        {
            _paymentInfo = paymentInfo;
        }
        public async Task<IActionResult> Index([FromQuery] FilterOptions filter, Guid id)
        {
            var payment = await _paymentInfo.GetPaginatedListAsync(filter, id);
            ViewBag.filter = filter;
            ViewBag.id = id;
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "", Url = Url.Action(nameof(Index), "Member") },
                new BreadCrumb {Title = ""}
            };
            return View(payment);
        }
        [HttpGet]
        public async Task<IActionResult> Create(Guid id)
        {
            ViewBag.id = id;
            ViewBag.Breadcrumbs = new List<BreadCrumb>
            {
                new BreadCrumb {Title = "List", Url = Url.Action(nameof(Index)) },
                new BreadCrumb {Title = "Create"}
            };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid Id, string month, string amount)
        {
            Month monthenum = (Month)Enum.Parse(typeof(Month), month, true);
            decimal amountdecimal = decimal.Parse(amount);

            var response = await _paymentInfo.CreateAsync(monthenum, amountdecimal, User.Identity?.Name ?? "", Id);
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index), "Member");
            }

            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index), "Member");
        }
       

        [HttpPost]
        public async Task<IActionResult> Edit(Guid memberId, string month, string amount)
        {
            Month monthenum = (Month)Enum.Parse(typeof(Month), month, true);
            decimal amountdecimal = decimal.Parse(amount);
            var response = await _paymentInfo.UpdateAsync(memberId, monthenum, amountdecimal, "Admin");
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index), "Member");
            }
            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index), "Member");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _paymentInfo.DeleteAsync(id, "Admin");
            if (response.Status)
            {
                TempData["success"] = response.Message;
                return RedirectToAction(nameof(Index), "Member");
            }
            TempData["error"] = response.Message;
            return RedirectToAction(nameof(Index), "Member");
        }
       
    }
}
