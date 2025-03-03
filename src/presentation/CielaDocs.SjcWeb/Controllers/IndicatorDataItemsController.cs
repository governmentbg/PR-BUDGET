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

namespace CielaDocs.SjcWeb.Controllers
{
    public class IndicatorDataItemsController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;
        private  FilterMainDataVm? FilterData = null;

        public IndicatorDataItemsController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor,
                        ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IWebHostEnvironment env,
                        ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
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
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(FilterData?.FunctionalSubAreaId ?? 0);
            ViewBag.Year = FilterData?.Nyear;
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.FunctionalSubAreaId = FilterData?.FunctionalSubAreaId ?? 0;
            ViewBag.IsLocked = FilterData?.IsLocked ?? false;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetDataGrid(int? nyear)
        {
            try
            {
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var data = await _sjcRepo.GetIndicatorData3YAsync(FilterData?.FunctionalSubAreaId ?? 0, nyear ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCourtsByIndicatorDataId(int? nyear, int? indicatorDataId)
        {
            var indicatorData = await _sjcService.QueryRaw<IndicatorDataVm>($"SELECT [Id],[MainIndicatorId] ,[FunctionalSubAreaId],[Nvalue],[EnteredDate],[PlannedYear],[ApprovedValue],[CalculatedValue],[BudgetPeriodId]  FROM [dbo].[IndicatorData] where id={indicatorDataId??0}");
            var mIndicator = await _sjcServiceV2.GetMainIndicatorsById(indicatorData?.MainIndicatorId??0);
            var data = await _sjcServiceV2.GetIndicatorDataCourt3YAsync(mIndicator?.FunctionalSubAreaId ?? 0, nyear ?? 0, indicatorData?.MainIndicatorId ?? 0);
            return Json(data.ToList());
        }
       

        [HttpPost]

        public async Task<JsonResult> UpdateDataItem(int key, string values)
        {
            dynamic objval = Newtonsoft.Json.JsonConvert.DeserializeObject(values);
            var dtype1 = objval.GetType();
            decimal n = 0;
            string name = string.Empty;
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            if (objval.GetType() == typeof(JObject))
            {
                foreach (var oelem in objval)
                {
                    name = oelem.Name;
                    decimal.TryParse(oelem.Value.ToString(), out n);
                }
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
     
                _ = await _sjcServiceV2.UpdateIndicatorData3YValueByIdAsync(key, name, n,FilterData?.Nyear);



            }
            return Json(string.Empty);
        }
        [HttpPost]
        public async Task<JsonResult> UpdateDataCourtItem(int key, string values)
        {
            dynamic objval = Newtonsoft.Json.JsonConvert.DeserializeObject(values);
            var dtype1 = objval.GetType();
            decimal n = 0;
            string name = string.Empty;
            if (objval.GetType() == typeof(JObject))
            {
                foreach (var oelem in objval)
                {
                    name = oelem.Name;
                    decimal.TryParse(oelem.Value.ToString(), out n);
                }
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                _ = await _sjcServiceV2.UpdateIndicatorDataCourt3YValueByIdAsync(key, name, n);
            }
            return Json(string.Empty);
        }
        public ActionResult CourtsInProgram(int? functionalSubAreaId)
        {

            ViewBag.FunctionalSubAreaId = functionalSubAreaId ?? 0;
            return PartialView("_CourtsInProgramPartialView");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCourtsInProgramData(int? functionalSubAreaId)
        {
            var data = await _sjcRepo.GetCourtsInProgramData(functionalSubAreaId);
            return Json(data.ToList());
        }
    }
}
