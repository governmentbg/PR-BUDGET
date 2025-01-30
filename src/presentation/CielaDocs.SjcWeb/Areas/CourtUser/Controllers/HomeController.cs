using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Helper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CielaDocs.SjcWeb.Areas.CourtUser.Controllers
{
    [Area("CourtUser")]
    [Authorize(Policy = "CourtUserOnly")]
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcBudgetRepository _sjcRepo;

        public HomeController(IMediator mediator, IMapper mapper, ILogRepository logRepo,  IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env, ISjcBudgetRepository sjcRepo)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _sjcRepo = sjcRepo;

        }
        public object GetUserId()
        {
            var id = _httpContextAccessor.HttpContext.User.Identity.Name;
            return id;
        }
        [AllowAnonymous]
        public IActionResult ErrorMessage(string message)
        {
            ViewBag.Message = message;
            return View("_ErrorMessage");
        }
        public async Task<IActionResult> Index()
        {
            int custType = User.GetUserTypeIdValue();

            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
           
            var court = await _mediator.Send(new GetCourtByIdQuery { Id = empl?.CourtId??0 });
            string? appMode = GlobalConfig.GetValue("ApplicationMode:AppMode");
            ViewBag.AppMode = (appMode?.ToLower() == "demo") ? "ДЕМО версия" : string.Empty;

            ViewBag.CourtName = court?.Name ?? string.Empty;
             ViewBag.CourtId= court?.Id??0;
             ViewBag.UserId=empl?.Id??0;
            return  View() ;
        }
        public IActionResult AddMainDataFilterPartial() => PartialView(nameof(AddMainDataFilterPartial));

        public IActionResult AddMainDataItemFilterPartial() => PartialView(nameof(AddMainDataItemFilterPartial));
        public IActionResult AddProgramDataFilterPartial()
        {
            return  PartialView(nameof(AddProgramDataFilterPartial));
        }
        [HttpPost]
        public async Task<JsonResult> SetMainDataFilter(int? functionalSubAreaId, int? courtId, int? nm, int? ny)
        {
            try
            {
                if ((functionalSubAreaId == null) || (functionalSubAreaId < 1) || (courtId == null) || (courtId < 1) || (nm == null) || (nm < 1) || (ny == null) || (ny < 2022))
                {
                    return Json(new { success = false, msg = "Не сте избрали коректни условия! " });
                }
                var mdexists = await _sjcRepo.CheckMainDataByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                var mditemsexists = await _sjcRepo.CheckMainDataItemsByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                if (!mdexists)
                {
                    _ = await _sjcRepo.SpLoadMainDataByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                }
                if (!mditemsexists)
                {
                    _ = await _sjcRepo.SpLoadMainDataItemsByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                }
                HttpContext.Session.Remove("FilterMainDataSess");
                HttpContext.Session.Set<FilterMainDataVm>("FilterMainDataSess", new FilterMainDataVm { FunctionalSubAreaId = functionalSubAreaId ?? 0, CourtId = courtId ?? 0, Nmonth = nm ?? 0, Nyear = ny ?? 0 });
                return Json(new { success = true, msg = "Ok" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Грешка: " + ex?.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> SetMainDataItemFilter(int? courtId, int? nm, int? ny)
        {
            try
            {
                if ((courtId == null) || (courtId < 1) || (nm == null) || (nm < 1) || (ny == null) || (ny < 2022))
                {
                    return Json(new { success = false, msg = "Не сте избрали коректни условия! " });
                }
                var mdexists = await _sjcRepo.CheckMainDataByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                var mditemsexists = await _sjcRepo.CheckMainDataItemsByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                if (!mdexists)
                {
                    _ = await _sjcRepo.SpLoadMainDataByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                }
                if (!mditemsexists)
                {
                    _ = await _sjcRepo.SpLoadMainDataItemsByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                }
                HttpContext.Session.Remove("FilterMainDataSess");
                HttpContext.Session.Set<FilterMainDataVm>("FilterMainDataSess", new FilterMainDataVm { FunctionalSubAreaId = 0, CourtId = courtId ?? 0, Nmonth = nm ?? 0, Nyear = ny ?? 0 });
                return Json(new { success = true, msg = "Ok" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Грешка: " + ex?.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> SetProgramDataFilter(int? functionalSubAreaId, int? ny, int? currencyId, int? currencyMeasureId)
        {
            try
            {
                if ((functionalSubAreaId == null) || (functionalSubAreaId < 1) || (ny == null) || (ny < 2022))
                {
                    return Json(new { success = false, msg = "Не сте избрали коректни условия! " });
                }
                int custType = User.GetUserTypeIdValue();

                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });

                var court = await _mediator.Send(new GetCourtByIdQuery { Id = empl?.CourtId ?? 0 });

                HttpContext.Session.Remove("FilterMainDataSess");
                HttpContext.Session.Set<FilterMainDataVm>("FilterMainDataSess", new FilterMainDataVm { FunctionalSubAreaId = functionalSubAreaId ?? 0, Nyear = ny ?? 0, CurrencyId = currencyId ?? 0, CurrencyMeasureId = currencyMeasureId ?? 0 });
                _ = await _sjcRepo.Sp_InitProgramDataCourtByIdAsync(functionalSubAreaId ?? 0,court?.Id??0, ny ?? 0);
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Филтър по програми {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
                return Json(new { success = true, msg = "Ok" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Грешка: " + ex?.Message });
            }
        }
    }
}
