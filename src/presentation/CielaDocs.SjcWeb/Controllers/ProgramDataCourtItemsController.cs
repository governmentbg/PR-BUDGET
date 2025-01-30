using CielaDocs.Application;
using CielaDocs.Application.Models;
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
    public class ProgramDataCourtItemsController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;

        private FilterMainDataVm? FilterData = null;
        private static int programNum=0;
        private static int CourtId = 0;
        public ProgramDataCourtItemsController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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
        public async Task<IActionResult> Index(int? programNum, int? courtId,int? nyear)
        {
            CourtId = courtId ?? 0;
            programNum = programNum ?? 0;
            var court= await _sjcRepo.GetNameByIdFromTable("Court", courtId);
           
            var prog = await _sjcRepo.GetFunctionalSubAreabyIdAsync(programNum ?? 0);
            var currencyName = await _sjcRepo.GetNameByIdFromTable("Currency", FilterData?.CurrencyId);
            var currencyMeasureName = await _sjcRepo.GetNameByIdFromTable("CurrencyMeasure", FilterData?.CurrencyMeasureId ?? 0);
            ViewBag.CurrencyName = currencyName;
            ViewBag.CurrencyMeasureName = currencyMeasureName;
            ViewBag.Year = nyear ?? 0;
            ViewBag.ProgramName = prog?.Name ?? string.Empty;
            ViewBag.FunctionalSubAreaId = programNum ?? 0;
            ViewBag.CourtName = court??string.Empty;
            ViewBag.CourtId = CourtId;
            ViewBag.ProgramNum = programNum;

            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Достъп до данни за бюджетни програми от {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetDataGrid(int? programNum, int? courtId, int? nyear)
        {
            try
            {
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var data = await _sjcRepo.GetProgramDataCourt3YByCourtIdAsync(programNum,nyear ?? 0,courtId??0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataCourtGridVm>());
            }
        }
        public async Task<JsonResult> RecalculateGrid(int? functionalSubAreaId, int? courtId, int? nyear)
        {
            try
            {
                _ = await _sjcRepo.sp_RecalculateProgramDataCourtAsync(functionalSubAreaId ?? 0, nyear ?? 0, courtId);
                _ = await _sjcRepo.sp_RecalculateProgramDataCourtAsync(functionalSubAreaId ?? 0, nyear + 1 ?? 0, courtId);
                _ = await _sjcRepo.sp_RecalculateProgramDataCourtAsync(functionalSubAreaId ?? 0, nyear + 2 ?? 0, courtId);
                _ = await _sjcRepo.sp_RecalculateProgramDataCourtAsync(functionalSubAreaId ?? 0, nyear + 3 ?? 0, courtId);

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
       
    }
}
