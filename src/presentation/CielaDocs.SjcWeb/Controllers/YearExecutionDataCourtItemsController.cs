using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.ExpressionEngine;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;

using ClosedXML.Excel;

using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;


using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Graph;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Data;

using static iText.Svg.SvgConstants;

namespace CielaDocs.SjcWeb.Controllers
{
    public class YearExecutionDataCourtItemsController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;

        private FilterMainDataVm? FilterData = null;
        private static int programNum = 0;
        private static List<int> court_Ids=new();
        private static int nYear=0;
        private static int selectedM1 = 0;
        private static int selectedM2 = 0;
        public YearExecutionDataCourtItemsController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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
        public async Task<IActionResult> Index(int? programNum, string courtIds, int? nyear,int?m1, int? m2 )
        {
           
            programNum = programNum ?? 0;
            court_Ids = courtIds.Split(',').Select(int.Parse).ToList(); 
            nYear=nyear ?? 0;
            selectedM1 = m1 ?? 0;
            selectedM2 = m2 ?? 0;

            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(FilterData?.FunctionalSubAreaId ?? 0);
            var currencyName = await _sjcRepo.GetNameByIdFromTable("Currency", FilterData?.CurrencyId);
            var currencyMeasureName = await _sjcRepo.GetNameByIdFromTable("CurrencyMeasure", FilterData?.CurrencyMeasureId ?? 0);
            ViewBag.CurrencyName = currencyName;
            ViewBag.CurrencyMeasureName = currencyMeasureName;
            ViewBag.Year = nyear ?? 0;
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.FunctionalSubAreaId = FilterData?.FunctionalSubAreaId ?? 0;
            ViewBag.CourtName =string.Join<string>(",", await _sjcRepo.GetCourtNamesByIds(court_Ids));
            ViewBag.ProgramNum = programNum;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetDataGrid(int? programNum)
        {
            try
            {
                var data = await _sjcRepo.GetProgramDataCourtGridByIdsAsync(programNum ?? 0, selectedM1, selectedM2, nYear, court_Ids);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataExecutionVm>());
            }
        }
      

    }
}
