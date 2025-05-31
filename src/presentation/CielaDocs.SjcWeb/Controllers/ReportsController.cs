

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Constants;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;

using ClosedXML.Excel;

using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;

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
    [Authorize]
    public class ReportsController : Controller
    {

        private readonly ILogger<HomeController> _logger;



        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;

        public ReportsController(ILogger<HomeController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo, 
                        ISjcBudgetRepository sjcRepo, IWebHostEnvironment env,ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _logger = logger;
            _mediator = mediator;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _env = env;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;

        }


        public async Task<IActionResult> Index()
        {
            ViewData["ActivePeriod"] = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
            ViewData["Cfg"] = await _sjcRepo.GetCfgAsync();
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Достъп до справки и отчети от {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

            return View();

        }

        public async Task<IActionResult> KontoReport(string par, int? currencyId) {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int institutionTypeId);
            int.TryParse(args[1], out int courtTypeId);
            int.TryParse(args[2], out int courtId);
            int.TryParse(args[3], out int nYear);
            int.TryParse(args[4], out int nMonth);
            int.TryParse(args[5], out int reportTypeId);
            ViewBag.InstitutionTypeId = institutionTypeId;
            ViewBag.CourtTypeId = courtTypeId;
            ViewBag.CourtId = courtId;
            ViewBag.Nyear = nYear;
            ViewBag.Nmonth = nMonth;
            ViewBag.ReportTypeId = reportTypeId;
            @ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetKontoData(int? institutionTypeId, int? courtTypeId, int? courtId, int? nyear, int? nmonth, int? reportTypeId,int? displayCurrencyId)
        {
            try
            {
                var data = await _sjcRepo.GetKontoCourtsYearCurrencyAsync(institutionTypeId, courtTypeId,courtId,nyear,nmonth,reportTypeId, displayCurrencyId??0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<KontoCourtsYearVm>());
            }
        }
        public async Task<IActionResult> ProgramExecutionReport(string par, int? currencyId) {
         
            string[] args = par.Split('|');
            int.TryParse(args[0], out int functionalSubAreaId);
            int.TryParse(args[1], out int nMonth);
            int.TryParse(args[2], out int nYear);
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(functionalSubAreaId);
        
            ViewBag.Year = nYear;
            ViewBag.Month = nMonth;
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.FunctionalSubAreaId = functionalSubAreaId;
            @ViewBag.Currency= await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            return View();
        }

        public IActionResult YearExecutionReport(string par, int? currencyId) {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int nM1);
            int.TryParse(args[1], out int nM2);
            int.TryParse(args[2], out int nYear);

           
            ViewBag.Year = nYear;
            ViewBag.M1 = nM1;
            ViewBag.M2 = nM2;
            ViewBag.Currency = BaseStore.Items[currencyId??0]?.Name;
            return View();
        }
        public async Task<IActionResult> InstitutionTypeYearExecutionReport(string par, int? currencyId) {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int institutionTypeId);
            int.TryParse(args[1], out int nYear);
            int.TryParse(args[2], out int selectedFnSubAreaId);
            var court = await _sjcRepo.GetNameByIdFromTable("InstitutionType", institutionTypeId);
            ViewBag.Year = nYear;
            ViewBag.InstitutionTypeId = institutionTypeId;
            ViewBag.FunctionalSubAreaId = selectedFnSubAreaId;
            ViewBag.CourtName = court ?? string.Empty;
            @ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            return View();
        }
        public IActionResult AddYearExecutionFilterPartial()=>PartialView(nameof(AddYearExecutionFilterPartial));
        public IActionResult AddCourtYearFilterPartial() => PartialView(nameof(AddCourtYearFilterPartial));
        public IActionResult AddProgramDataFilterPartial() => PartialView(nameof(AddProgramDataFilterPartial));
        public IActionResult AddInstitutionYearFilterPartial() => PartialView(nameof(AddInstitutionYearFilterPartial));
        public IActionResult AddProgramYearFilterPartial() => PartialView(nameof(AddProgramYearFilterPartial));
        public IActionResult AddCommonBudgetFilterPartial() => PartialView(nameof(AddCommonBudgetFilterPartial));
        public IActionResult EndedBudgetPeriodFilterPartial()=> PartialView(nameof(EndedBudgetPeriodFilterPartial));
        public IActionResult AddMainDataFilterPartial() => PartialView(nameof(AddMainDataFilterPartial));
        public IActionResult AddImportKontoFilterPartial() => PartialView(nameof(AddImportKontoFilterPartial));

        [HttpGet]

        public async Task<JsonResult> GetProgramExecutionDataGrid(int? functionalSubAreaId, int? nm, int? ny,int? displayCurrencyId)
        {
            try
            {
               
                var data = await _sjcRepo.GetProgramDataGridByFilterCurrencyAsync(functionalSubAreaId ?? 0, ny??0, displayCurrencyId??0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCourtsByProgramExecutionDataId(int? programDataId,int? displayCurrencyId)
        {
            var prog = await _sjcRepo.GetProgramDataByIdAsync(programDataId);
            var data = await _sjcRepo.GetProgramDataCourtGridByFilterCurrencyAsync(prog?.ProgramDefNum, prog?.PlannedYear, prog?.RowNum, displayCurrencyId ?? 0);
            return Json(data.ToList());
        }

        [HttpGet]
        public async Task<PartialViewResult> YearExecutionPartialView(int? functionalSubAreaId)
        {
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(functionalSubAreaId??0);
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            return PartialView("YearExecutionPartialView");
        }
        [HttpGet]

        public async Task<JsonResult> GetYearExecutionDataGrid(int? functionalSubAreaId, int? m1, int? m2, int? nyear, int? displayCurrencyId)
        {
            try
            {
                var data = await _sjcRepo.GetYearExecutionDataGridAsync(functionalSubAreaId ?? 0,m1,m2, nyear ?? 0, displayCurrencyId);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataExecutionVm>());
            }
        }
       
        [HttpGet]
        public async Task<JsonResult> GetYearExecutionCourtsByProgramDataId(int? m1, int? m2, int? nyear,int? programDataId, int? displayCurrencyId)
        {
            var prog = await _sjcRepo.GetProgramDataByIdAsync(programDataId);
            var data = await _sjcRepo.GetProgramDataCourtGridByFilterAsync(prog?.ProgramDefNum, m1,m2, prog?.PlannedYear, prog?.RowNum, displayCurrencyId);
            return Json(data.ToList());
        }
        public ActionResult CourtsInProgram(int? functionalSubAreaId)
        {

            ViewBag.FunctionalSubAreaId = functionalSubAreaId ?? 0;
            return PartialView("_CourtsInYearExecutionPartialView");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCourtsInProgramData(int? functionalSubAreaId)
        {
            var data = await _sjcRepo.GetCourtsInProgramData(functionalSubAreaId);
            return Json(data.ToList());
        }
        public async Task<IActionResult> FunctionalSubAreaNumYearReport(string par,int? currencyId)
        {
            string[] args = par.Split('|');
            string[] reps = new string[] { "За отчетни единици", "Експертен бюджет" };
            int.TryParse(args[0], out int reportTypeId);
            int.TryParse(args[1], out int nYear);
            int.TryParse(args[2], out int selectedFnSubAreaId);
            ViewBag.Year = nYear;
            ViewBag.ReportTypeId = reportTypeId;
            ViewBag.FunctionalSubAreaId = selectedFnSubAreaId;
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(selectedFnSubAreaId);
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.ReportTypeName = reps[reportTypeId];
            @ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId??0);
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetProgramNumDataGrid(int? functionalSubAreaId, int? reportTypeId, int? ny, int? displayCurrencyId)
        {
            try
            {
                if (reportTypeId == 1)
                {
                    var data = await _sjcRepo.GetProgramDataInstitution3YCommonCurrencyAsync(functionalSubAreaId ?? 0, ny ?? 0,displayCurrencyId??0);
                    return Json(data.ToList());
                }
                else {
                    var data = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(functionalSubAreaId ?? 0, ny ?? 0, displayCurrencyId ?? 0);
                    return Json(data.ToList());
                }

            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }
        public async Task<IActionResult> Indicators(string par, int? currencyId)
        {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int functionalSubAreaId);
            int.TryParse(args[1], out int nMonth1);
            int.TryParse(args[2], out int nMonth2);
            int.TryParse(args[3], out int nYear);


           
            var fsub = await _mediator.Send(new GetFunctionalSubAreaByIdQuery { Id = functionalSubAreaId });

           
            ViewBag.FunctionalSubAreaId = functionalSubAreaId;
            ViewBag.Month1 = nMonth1;
            ViewBag.Month2 = nMonth2;
            ViewBag.Year = nYear;
            ViewBag.FunctionalSubAreaName = fsub?.Name;
            ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            return View();
        }

        public async Task<IActionResult> AnalizeImportKonto(string par, int? currencyId)
        {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int functionalSubAreaId);
            int.TryParse(args[1], out int institutionTypeId);
            int.TryParse(args[2], out int courtTypeId);
            int.TryParse(args[3], out int courtId);
            int.TryParse(args[4], out int nMonth);
            int.TryParse(args[5], out int nYear);
            string kontoCode = args[6]??string.Empty;
            string filterTitle=string.Empty;
            if (functionalSubAreaId > 0) {
                var fsub = await _mediator.Send(new GetFunctionalSubAreaByIdQuery { Id = functionalSubAreaId });
                filterTitle+= $"Програма: {fsub?.Name}";
            }
            if (courtId > 0) { 
                filterTitle += $" , Съд: {await _sjcRepo.GetNameByIdFromTable("Court", courtId)}";
            }
            if (nMonth > 0) { 
                filterTitle += $" , Месец: {nMonth}";
            }
            if (nYear > 0) { 
                filterTitle += $" , Година: {nYear}";
            }
            if (!string.IsNullOrWhiteSpace(kontoCode)) { 
                filterTitle += $" , Код: {kontoCode}";
            }
            ViewBag.FunctionalSubAreaId = functionalSubAreaId;
            ViewBag.Month = nMonth;
            ViewBag.Year = nYear;
            ViewBag.CourtId=courtId;
            ViewBag.KontoCode = kontoCode;
            ViewBag.FilterTitle = filterTitle;
            ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetImportedKontoData(int? functionalSubAreaId,  int? courtId, int? nyear, int? nmonth, string? kontoCode, int? displayCurrencyId)
        {
            try
            {
                string sql = $@"Select a.Id,a.CourtId,a.FunctionalSubAreaId,a.Nmonth,a.Nyear,a.Nvalue,a.KontoCode,c.KontoCode as CourtKontoCode, c.Name as CourtName,f.NAme as FunctionalSubAreaName
                          from CourtInKontoCode a
                          left join Court c on a.courtId=c.id
                         left join FunctionalSubArea f on a.FunctionalSubAreaId=f.id 
                         where a.Id>0 ";
                if (functionalSubAreaId > 0) { 
                    sql += $" and a.FunctionalSubAreaId={functionalSubAreaId} ";
                }
                if (courtId > 0) { 
                    sql += $" and a.CourtId={courtId} ";
                }
                if (nyear > 0) { 
                    sql += $" and a.Nyear={nyear} ";
                }
                if (nmonth > 0) { 
                    sql += $" and a.Nmonth={nmonth} ";
                }
                if (!string.IsNullOrWhiteSpace(kontoCode)) { 
                    sql += $" and a.KontoCode = '{kontoCode}' ";
                }
                var data = await _sjcService.QueryRawList<CourtInKontoCodeVm>(sql);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtInKontoCodeVm>());
            }
        }
    }
}
