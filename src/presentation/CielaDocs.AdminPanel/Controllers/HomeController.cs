using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CielaDocs.AdminPanel.Models;
using CielaDocs.Application.Dtos;
using CielaDocs.Application.Models;
using CielaDocs.Application;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using CielaDocs.Shared.Services;
using CielaDocs.AdminPanel.Extensions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CielaDocs.AdminPanel.Controllers;
[Authorize(Policy = "AdminOnly")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;


    private readonly IMediator _mediator;
    private readonly ISendGridMailer _emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogRepository _logRepo;
    private readonly ISjcBudgetRepository _sjcRepo;
    private readonly IWebHostEnvironment _env;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IWebHostEnvironment env)
    {
        _logger = logger;
        _mediator = mediator;
        _emailSender = emailSender;
        _httpContextAccessor = httpContextAccessor;
        _logRepo = logRepo;
        _sjcRepo = sjcRepo;

        _env = env;

    }

   
    [Route("/Home/HandleError/{code:int}")]
    public IActionResult HandleError(int code)
    {
        ViewData["ErrorMessage"] = $"Нямате права за достъп до този ресурс: {code}";
        return View("~/Views/Shared/HandleError.cshtml");
    }
   
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
       
        if (User?.Identity?.IsAuthenticated??false)
        {
           return RedirectToAction("Index","Home",new {area="Admin" });

        }
        return View();

    }
    public IActionResult License(string id)
    {
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
    public async Task<PartialViewResult> GetEmplLogReportFilter(int? emplId)
    {
        var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });

        EmplLogVm model = new EmplLogVm();
        model.EmplId = empl?.Id ?? 0;
        model.ReportGuid = Guid.NewGuid().ToString("N");
        return PartialView("EmplLogFilterPartialView", model);
    }
    [HttpPost]
    public async Task<JsonResult> EmplLogFilter(EmplLogVm model)
    {
        var empl = await _mediator.Send(new GetUserExtByIdQuery { Id = model.EmplId });
        string sReportTitle = $"Потребителски действия на потребител {empl?.FirstName}";
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
    [AllowAnonymous]
    public IActionResult LoadEmplLogReportFilterGrid(string reportGuid)
    {
        return ViewComponent("EmplLogResponseGrid", new { reportGuid = reportGuid });
    }
    
}

