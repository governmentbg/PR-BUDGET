using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Helper;
using CielaDocs.SjcWeb.Models;

using ClosedXML.Excel;

using DocumentFormat.OpenXml.Bibliography;

using FluentValidation.Internal;

using Google.Protobuf;

using gRpcFileTransfer;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CielaDocs.SjcWeb.Controllers
{
    //[Authorize]
    [Authorize]
    public class HomeController : Controller
    {
        private const int ChunkSize = 1024 * 32; // 32 KB
        private readonly ILogger<HomeController> _logger;



        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IWebHostEnvironment env)
        {
            _logger = logger;
            _mediator = mediator;
             _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _env =env;
            _config = configuration;

        }

        [AllowAnonymous]
        public IActionResult AuthorshipPartial() { 
            return PartialView("AuthorshipPartial");
        }
        [AllowAnonymous]
        public IActionResult Feedback()
        {
            
            return View(new FeedbackDto());
        }
        [Route("/Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            ViewData["ErrorMessage"] = $"Нямате права за достъп до този ресурс: {code}";
            return View("~/Views/Shared/HandleError.cshtml");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeedback(FeedbackDto model)
        {
            ViewBag.Comment = string.Empty;

            if (!ModelState.IsValid)
            {
                ViewBag.Msg = "Невалидни данни!";
                ModelState.AddModelError("", "Попълнете правилно полетата във формата");
                return View("Feedback");
            }
          
            var ret = await _mediator.Send(new CreateFeedbackCommand { Name = model?.Name, Email = model?.Email, Notes = model.Notes });

            await _emailSender.SendFeedbackEmailAsync($"Обратна връзка, Подател {model?.Name}, имейл {model.Email}.", $"Текст: {model?.Notes}.");
            ViewBag.Comment = (ret >0) ? "Данните бяха изпратени" : "Възникна грешка при запис на вашите данни!";
            return View("Feedback");
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            string? appMode= GlobalConfig.GetValue("ApplicationMode:AppMode");
            ViewBag.AppMode = (appMode?.ToLower()=="demo")?"ДЕМО версия":string.Empty;
            if (User?.Identity?.IsAuthenticated ?? false)
            {
               
                int custType = User.GetUserTypeIdValue();
               
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Вход в системата на {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

                var onr = await _mediator.Send(new GetCourtByIdQuery { Id = empl?.CourtId ?? 0 });
                if (empl?.LoginEnabled == true)
                {
                    ViewBag.UserId = empl.Id;
                    ViewBag.CourtId = empl?.CourtId ?? 0;
                    ViewBag.CourtName = onr?.Name;
                }
                else return RedirectToAction("AccessDenied", "Account");

                if ((custType == 2)||(custType==3) || (custType == 4)) {
                    return RedirectToAction("Index", "Home",new {area="CourtUser" });
                }
            }
            return View();

        }
        public IActionResult AddMainDataFilterPartial() => PartialView(nameof(AddMainDataFilterPartial)); 
        
        public IActionResult AddMainDataItemFilterPartial()=> PartialView(nameof(AddMainDataItemFilterPartial));
        public IActionResult AddProgramDataFilterPartial() => PartialView(nameof(AddProgramDataFilterPartial));
        public IActionResult AddPeriodDataItemFilterPartial() => PartialView(nameof(AddPeriodDataItemFilterPartial));
        public IActionResult AddMainDataPeriodFilterPartial() => PartialView(nameof(AddMainDataPeriodFilterPartial));
        public IActionResult AddApprovedDataItemFilterPartial() => PartialView(nameof(AddApprovedDataItemFilterPartial));
        public IActionResult NotSupportedFile()=>View(nameof(NotSupportedFile));

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            return View(empl);
        }
        public IActionResult License(string id) { 
            ViewBag.LicenseInfo = id;
            return View();
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NoLoginEnabled()
        {
            return View();
        }

       
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
      
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult ErrorMessage(string message, string debug)
        {
            ViewBag.Message = message;
            ViewBag.Debug = debug;
            return View("_ErrorMessage");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult ErrorWithMessage(string message, string debug)
        {
            return View("_ErrorMessage").WithError(message, debug);
        }
       
        [AllowAnonymous]
        public async  Task<PartialViewResult> GetEmplLogReportFilter()
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });

            EmplLogVm model = new EmplLogVm();
            model.EmplId = empl?.Id??0;
            model.ReportGuid = Guid.NewGuid().ToString("N");
            return PartialView("EmplLogFilterPartialView", model);
        }
        [HttpPost]
        public async Task<JsonResult> SetMainDataFilter(int? functionalSubAreaId,int? courtId, int? nm, int? ny) {
            try
            {
                if ((functionalSubAreaId==null)||(functionalSubAreaId<1)||(courtId == null) || (courtId < 1) || (nm == null) || (nm < 1) || (ny == null) || (ny < 2022)) {
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
                HttpContext.Session.Set<FilterMainDataVm>("FilterMainDataSess", new FilterMainDataVm { FunctionalSubAreaId= functionalSubAreaId??0, CourtId = courtId ?? 0, Nmonth = nm ?? 0, Nyear = ny ?? 0 });

                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Филтър с условие {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
                return Json(new { success = true, msg="Ok" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg="Грешка: "+ex?.Message });
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
        public async Task<JsonResult> SetPeriodDataItemFilter(int? courtId, int? nm, int? ny)
        {
            try
            {
                if ((courtId == null) || (courtId < 1) || (nm == null) || (nm < 1) || (ny == null) || (ny < 2022))
                {
                    return Json(new { success = false, msg = "Не сте избрали коректни условия! " });
                }
                var mdexists = await _sjcRepo.CheckPeriodDataByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                var mditemsexists = await _sjcRepo.CheckPeriodDataItemsByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                //if (!mdexists)
                //{
                //    _ = await _sjcRepo.SpLoadMainPeriodByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                //}
                //if (!mditemsexists)
                //{
                //    _ = await _sjcRepo.SpLoadMainPeriodItemsByCourtIdPeriodAsync(courtId ?? 0, nm ?? 0, ny ?? 0);
                //}
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
        public async Task<JsonResult> SetProgramDataFilter(int? functionalSubAreaId,  int? ny, int? currencyId, int? currencyMeasureId)
        {
            try
            {
                if ((functionalSubAreaId == null) || (functionalSubAreaId < 1) || (ny == null) || (ny < 2022))
                {
                    return Json(new { success = false, msg = "Не сте избрали коректни условия! " });
                }
               
                HttpContext.Session.Remove("FilterMainDataSess");
                HttpContext.Session.Set<FilterMainDataVm>("FilterMainDataSess", new FilterMainDataVm { FunctionalSubAreaId = functionalSubAreaId ?? 0,  Nyear = ny ?? 0 ,CurrencyId=currencyId??0, CurrencyMeasureId=currencyMeasureId??0});
                _ = await _sjcRepo.Sp_InitProgramDataAsync(functionalSubAreaId ?? 0, ny ?? 0);
                _ = await _sjcRepo.Sp_InitProgramDataCourtAsync(functionalSubAreaId ?? 0, ny ?? 0);
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
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
        [HttpPost]
        public async Task<JsonResult> EmplLogFilter(EmplLogVm model)
        {
            var empl = await _mediator.Send(new GetUserByIdQuery { Id = model.EmplId });
            string sReportTitle = $"Потребителски действия на потребител {empl?.UserName}";
            string sReportSubTitle = string.Empty;
            string sql = string.Empty;



            if ((model.StartDate != null) && (model.EndDate != null))
            {
                sql += $"SELECT * FROM Ulog WHERE CreatedOn>='{Toolbox.GetSqlDateTime((DateTime)model.StartDate, 1)}' and CreatedOn<='{Toolbox.GetSqlDateTime((DateTime)model.EndDate, 1)}' and EmplId = {model.EmplId}";
                sReportSubTitle += $"Регистрирани действия от {Toolbox.GetBGDateTime((DateTime)model.StartDate, 1)} до {Toolbox.GetBGDateTime((DateTime)model.EndDate, 1)}";


                var ret1 = await _mediator.Send(new CreateOnrFilterLogReportCommand { ReportGuid = model?.ReportGuid ?? string.Empty, ReportCondition = sql, ReportTitle = sReportTitle, ReportSubTitle = sReportSubTitle });

                return Json(new { Id = (int)ret1, ReportGuid = model?.ReportGuid, success = true }); ;
            }
            else
            {
                return Json(new { Id = 0, ReportGuid = model?.ReportGuid, success = false, msg = "Не сте избрали начална и крайна дата за отчета" });
            }
        }


       
             [HttpPost]
        public async Task<JsonResult> SetApprovedDataFilter(int? functionalSubAreaId, int? ny, int? currencyId, int? currencyMeasureId)
        {
            try
            {
                if ((functionalSubAreaId == null) || (functionalSubAreaId < 1) || (ny == null) || (ny < 2022))
                {
                    return Json(new { success = false, msg = "Не сте избрали коректни условия! " });
                }

                HttpContext.Session.Remove("FilterMainDataSess");
                HttpContext.Session.Set<FilterMainDataVm>("FilterMainDataSess", new FilterMainDataVm { FunctionalSubAreaId = functionalSubAreaId ?? 0, Nyear = ny ?? 0, CurrencyId = currencyId ?? 0, CurrencyMeasureId = currencyMeasureId ?? 0 });
                _ = await _sjcRepo.Sp_InitProgramDataAsync(functionalSubAreaId ?? 0, ny ?? 0);
                _ = await _sjcRepo.Sp_InitProgramDataCourtAsync(functionalSubAreaId ?? 0, ny ?? 0);
                return Json(new { success = true, msg = "Ok" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Грешка: " + ex?.Message });
            }
        }
      

        [AllowAnonymous]
        public IActionResult LoadEmplLogReportFilterGrid(string reportGuid)
        {
            return ViewComponent("EmplLogResponseGrid", new { reportGuid = reportGuid });
        }
        [AllowAnonymous]
        public async Task<JsonResult> GetEmplLogFilter(string reportGuid, int? page, int? limit, string sortBy, string direction, string name)
        {
            IEnumerable<Ulog> records;

            int total = 0;
            var querystr = await _mediator.Send(new GetReportConditionByReportGuidQuery { ReportGuid = reportGuid ?? String.Empty });
            var query = await _logRepo.GetUserLogByEmplAsync(querystr);
            total = query.Count();
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(direction))
            {
                if (direction.Trim().ToLower() == "asc")
                {
                    switch (sortBy.Trim().ToLower())
                    {
                        case "createdon":
                            query = query.OrderBy(q => q.CreatedOn);
                            break;
                    }
                }
                else
                {
                    switch (sortBy.Trim().ToLower())
                    {

                        case "createdon":
                            query = query.OrderByDescending(q => q.CreatedOn);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderByDescending(q => q.CreatedOn);
            }
            if (page.HasValue && limit.HasValue)
            {
                int start = (page.Value - 1) * limit.Value;
                records = (IEnumerable<Ulog>)query.Skip(start).Take(limit.Value).ToList();
            }
            else
            {
                records = (IEnumerable<Ulog>)query.ToList();
            }
            var settings = new JsonSerializerSettings() { DateFormatString = "dd.MM.yyyy HH:mm:ss" };
            return Json(new { records, total }, settings);
        }
       
       
       
    }
}
