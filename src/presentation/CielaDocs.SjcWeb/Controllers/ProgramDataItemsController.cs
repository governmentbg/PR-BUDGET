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
    public class ProgramDataItemsController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;

        private FilterMainDataVm? FilterData = null;

        public ProgramDataItemsController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(FilterData?.FunctionalSubAreaId ?? 0);
            var currencyName = await _sjcRepo.GetNameByIdFromTable("Currency", FilterData?.CurrencyId);
            var currencyMeasureName = await _sjcRepo.GetNameByIdFromTable("CurrencyMeasure", FilterData?.CurrencyMeasureId ?? 0);
            ViewBag.CurrencyName = currencyName;
            ViewBag.CurrencyMeasureName = currencyMeasureName;
            ViewBag.Year = FilterData?.Nyear;
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.FunctionalSubAreaId = FilterData?.FunctionalSubAreaId ?? 0;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetDataGrid(int? nyear)
        {
            try
            {
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var data = await _sjcRepo.GetProgramData3YAsync(FilterData?.FunctionalSubAreaId ?? 0, nyear ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCourtsByProgramDataId(int? nyear, int? programDataId)
        {
            var prog = await _sjcRepo.GetProgramDataByIdAsync(programDataId);
            var data = await _sjcRepo.GetProgramDataCourt3YAsync(prog?.ProgramDefNum ?? 0, nyear ?? 0, prog?.RowNum);
            return Json(data.ToList());
        }
        public async Task<JsonResult> RecalculateGrid(int? functionalSubAreaId, int? nyear)
        {
            try
            {
                _ = await _sjcRepo.sp_RecalculateProgramDataAsync(functionalSubAreaId ?? 0, nyear ?? 0);
                _ = await _sjcRepo.sp_RecalculateProgramDataAsync(functionalSubAreaId ?? 0, nyear + 1 ?? 0);
                _ = await _sjcRepo.sp_RecalculateProgramDataAsync(functionalSubAreaId ?? 0, nyear + 2 ?? 0);

                return Json(new { error = string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        private string ReplaceCalculationFormula(string Source, Dictionary<string, string> dic)
        {
            // string result=string.Empty;
            foreach (var (key, value) in dic)
            {
                int Place = Source.IndexOf(key);
                Source = Source.Remove(Place, key.Length).Insert(Place, value);
            }

            return Source;
        }

        [HttpPost]

        public async Task<JsonResult> UpdateDataItem(int key, string values)
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
                // _ = await _sjcRepo.UpdateProgramDataValueByIdAsync(key,name, n);

                _ = await _sjcRepo.UpdateProgramData3YValueByIdAsync(key, name, n);



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
                _ = await _sjcRepo.UpdateProgramDataCourt3YValueByIdAsync(key, name, n);
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
