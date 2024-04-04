

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
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


        public ReportsController(ILogger<HomeController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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


        public async Task<IActionResult> Index()
        {
            
            return View();

        }

        public IActionResult KontoReport(string par) {
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
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetKontoData(int? institutionTypeId, int? courtTypeId, int? courtId, int? nyear, int? nmonth, int? reportTypeId)
        {
            try
            {
                
                var data = await _sjcRepo.GetKontoCourtsYearAsync(institutionTypeId, courtTypeId,courtId,nyear,nmonth,reportTypeId);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<KontoCourtsYearVm>());
            }
        }
        public async Task<IActionResult> ProgramExecutionReport(string par) {
            //functionalSubAreaId,"|",mn, "|", ny,"|",currencyId,"|",currencyMeasureId
         
            string[] args = par.Split('|');
            int.TryParse(args[0], out int functionalSubAreaId);
            int.TryParse(args[1], out int nMonth);
            int.TryParse(args[2], out int nYear);
            int.TryParse(args[3], out int currencyId);
            int.TryParse(args[4], out int currencyMeasureId);
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(functionalSubAreaId);
            var currencyName = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            var currencyMeasureName = await _sjcRepo.GetNameByIdFromTable("CurrencyMeasure", currencyMeasureId);
            ViewBag.CurrencyName = currencyName;
            ViewBag.CurrencyMeasureName = currencyMeasureName;
            ViewBag.Year = nYear;
            ViewBag.Month = nMonth;
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.FunctionalSubAreaId = functionalSubAreaId;
            return View();
        }

        public IActionResult YearExecutionReport(string par) {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int nM1);
            int.TryParse(args[1], out int nM2);
            int.TryParse(args[2], out int nYear);

           
            ViewBag.Year = nYear;
            ViewBag.M1 = nM1;
            ViewBag.M2 = nM2;
            return View();
        }
        public async Task<IActionResult> InstitutionTypeYearExecutionReport(string par) {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int institutionTypeId);
            int.TryParse(args[1], out int nYear);
            int.TryParse(args[2], out int selectedFnSubAreaId);
            var court = await _sjcRepo.GetNameByIdFromTable("InstitutionType", institutionTypeId);
            ViewBag.Year = nYear;
            ViewBag.InstitutionTypeId = institutionTypeId;
            ViewBag.FunctionalSubAreaId = selectedFnSubAreaId;
            ViewBag.CourtName = court ?? string.Empty;
            return View();
        }
        public IActionResult AddYearExecutionFilterPartial()=>PartialView(nameof(AddYearExecutionFilterPartial));
        public IActionResult AddCourtYearFilterPartial() => PartialView(nameof(AddCourtYearFilterPartial));
        public IActionResult AddProgramDataFilterPartial() => PartialView(nameof(AddProgramDataFilterPartial));
        public IActionResult AddInstitutionYearFilterPartial() => PartialView(nameof(AddInstitutionYearFilterPartial));
        public IActionResult AddProgramYearFilterPartial() => PartialView(nameof(AddProgramYearFilterPartial));

        [HttpGet]

        public async Task<JsonResult> GetProgramExecutionDataGrid(int? functionalSubAreaId, int? nm, int? ny)
        {
            try
            {
               
                var data = await _sjcRepo.GetProgramDataGridByFilterAsync(functionalSubAreaId ?? 0, ny??0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCourtsByProgramExecutionDataId(int? programDataId)
        {
            var prog = await _sjcRepo.GetProgramDataByIdAsync(programDataId);
            var data = await _sjcRepo.GetProgramDataCourtGridByFilterAsync(prog?.ProgramDefNum, prog?.PlannedYear, prog?.RowNum);
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

        public async Task<JsonResult> GetYearExecutionDataGrid(int? functionalSubAreaId, int? m1, int? m2, int? nyear)
        {
            try
            {
                var data = await _sjcRepo.GetYearExecutionDataGridAsync(functionalSubAreaId ?? 0,m1,m2, nyear ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataExecutionVm>());
            }
        }
       
        [HttpGet]
        public async Task<JsonResult> GetYearExecutionCourtsByProgramDataId(int? m1, int? m2, int? nyear,int? programDataId)
        {
            var prog = await _sjcRepo.GetProgramDataByIdAsync(programDataId);
            var data = await _sjcRepo.GetProgramDataCourtGridByFilterAsync(prog?.ProgramDefNum, m1,m2, prog?.PlannedYear, prog?.RowNum);
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
        public async Task<IActionResult> FunctionalSubAreaNumYearReport(string par)
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
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetProgramNumDataGrid(int? functionalSubAreaId, int? reportTypeId, int? ny)
        {
            try
            {
                if (reportTypeId == 1)
                {
                    var data = await _sjcRepo.GetProgramDataInstitution3YCommonAsync(functionalSubAreaId ?? 0, ny ?? 0);
                    return Json(data.ToList());
                }
                else {
                    var data = await _sjcRepo.GetProgramDataCourt3YCommonAsync(functionalSubAreaId ?? 0, ny ?? 0);
                    return Json(data.ToList());
                }

            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }
    }
}
