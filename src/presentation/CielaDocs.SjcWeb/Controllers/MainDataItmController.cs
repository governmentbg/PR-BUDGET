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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;


using Newtonsoft.Json;


using System.Data;


namespace CielaDocs.SjcWeb.Controllers
{
    public class MainDataItmController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;

        private FilterMainDataVm? FilterData = null;

        public MainDataItmController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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
            var court = await _mediator.Send(new GetCourtByIdQuery { Id = FilterData?.CourtId ?? 0 });
            ViewData["court"] = court;
            ViewBag.Month = FilterData?.Nmonth;
            ViewBag.Year = FilterData?.Nyear;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetMainDataGrid()
        {
            try
            {
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var data = await _sjcRepo.GetMainDataItemsGridByFilterAsync(FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemsGrid>());
            }
        }

        [HttpGet]
        public async Task<PartialViewResult> GetMainDataItemsPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател!");
            }
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var md = await _sjcRepo.GetMainDataByIdAsync(id ?? 0);
            var mi = await _sjcRepo.GetMainIndicatorsByIdAsync(md?.MainIndicatorsId ?? 0);
            List<string> vars = new();
            var t = new Tokenizer(new StringReader(mi?.Calculation ?? string.Empty));
            while (t.Token != Token.EOF)
            {
                if (vars.IndexOf(t.Identifier) < 0)
                { vars.Add(t.Identifier); }

                t.NextToken();
            }
            var metricsFields = await _sjcRepo.GetMetricsFiledByCode(FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0, string.Join(',', vars.ToArray()));
            switch (vars.Count)
            {
                case 0:
                    return PartialView("MainDataItem0PartialView");

                case 1:
                case 2:
                case 3:
                case 4:
                    ViewBag.MainDataId = id ?? 0;
                    ViewBag.MainIndicatorsId = md?.MainIndicatorsId ?? 0;
                    return PartialView("MainDataItemsPartialView", metricsFields);


                default:
                    return PartialView("MainDataItem0PartialView");
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
        [HttpGet]
        public PartialViewResult UploadFilePartial()=> PartialView("UploadFilePartial");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMainDataItems(IEnumerable<MainDataItemsResult> record)
        {

            if (ModelState.IsValid)
            {

                try
                {
                    int.TryParse(Request.Form["MainDataId"].ToString(), out int nMainDataId);
                    int.TryParse(Request.Form["MainIndicatorsId"].ToString(), out int nMainIndicatorsId);
                    _ = await _sjcRepo.UpdateMainDataItemByIdAsync(record);

                    //---------------------calc------------------
                    var mi = await _sjcRepo.GetMainIndicatorsByIdAsync(nMainIndicatorsId);


                    var dic = new Dictionary<string, string>();
                    foreach (var itm in record)
                    {
                        if (!dic.ContainsKey(itm.Code))
                        {
                            dic.Add(itm.Code, itm.Nvalue.ToString());
                        }
                    }
                    string calculationString = ReplaceCalculationFormula(mi.Calculation ?? string.Empty, dic);

                    var res = Parser.Parse(calculationString).Eval(null);
                    if (mi?.MeasureId == 1)
                    {
                        res = res * 100;
                    }
                    var ok = await _sjcRepo.UpdateMainDataValueByIdAsync(nMainDataId, res);

                    return Json(new { msg = "Данните бяха редактирани", success = true });
                }
                catch (Exception ex)
                {
                    string messages = string.Join("; ", ModelState.Values
               .SelectMany(x => x.Errors)
               .Select(x => x.ErrorMessage));
                    return Json(new { msg = messages, success = false });
                }


            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
                return Json(new { msg = messages, success = false });
            }
        }
        [HttpPost]
    
        public async Task<JsonResult> UpdateMainDataItem(int key, string values)
        {
            var nv = new Nvalues();
            JsonConvert.PopulateObject(values, nv);
            _ = await _sjcRepo.UpdateMainDataItemValueByIdAsync(key, nv.Nvalue);
            return Json(string.Empty);
        }
       // [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> LoadCustomExcelFile(string id,bool? isOverwrite) {
            try
            {
                // Check the File is received

                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { msg = "Невалиден файл за зареждане на данни", success = false });

                string file = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
                var supportedTypes = new[] { "xlsm","xls", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(id).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    return Json(new { msg = "Невалиден файл за зареждане на данни. Задължително изберете файл с разширение .xlsx или .xls", success = false });
                }
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var dbdata = await _sjcRepo.GetMainDataItemsGridByFilterAsync(FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0);
                var data = Toolbox.ExcelToDataTable(file);
                int nCnt = 0;
                foreach (DataTable thisTable in data.Tables)
                {
                    // For each row, print the values of each column.
                    foreach (DataRow row in thisTable.Rows)
                    {
                        foreach (DataColumn column in thisTable.Columns)
                        {
                            var code = row["Код"].ToString();
                            decimal.TryParse(row["Стойност"]?.ToString(), out decimal nVal);
                            var rec = dbdata.Where(x => x.Code == code).FirstOrDefault();
                            if (rec != null) {
                                if (rec?.EnteredOn == null) {
                                    _ = await _sjcRepo.UpdateMainDataItemValueByIdAsync(rec?.Id, nVal);
                                    nCnt++;
                                }
                                else if (isOverwrite ?? false) {
                                    _ = await _sjcRepo.UpdateMainDataItemValueByIdAsync(rec?.Id, nVal);
                                    nCnt++;
                                }
                            }
                            
                        }
                    }
                }
                return Json(new { msg = $"Бяха заредени данни за {nCnt} записа", success = true });


            }
            catch (Exception ex)
            {
                return Json(new { msg = "Грешка при четене на файл: "+ex?.Message, success = false });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoadFromPrevMonth(int nm, int ny) {
           
            if (nm == 0 || ny == 0) return Json(new { msg = "Неинициализирани входни данни ", success = false });
            int prevM = 0;int prevY = 0;int nCnt = 0;
            if (nm == 1)
            {
                prevM = 12;
                prevY = ny - 1;
            }
            else {
                prevM = nm - 1;
                prevY = ny;
            }
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var dbdata = await _sjcRepo.GetMainDataItemsGridByFilterAsync(FilterData?.CourtId ?? 0, nm, ny);
            var prevdata = await _sjcRepo.GetMainDataItemsGridByFilterAsync(FilterData?.CourtId ?? 0, prevM, prevY);
            if (prevdata.Any()) {
                foreach (var item in prevdata)
                {
                    if(item?.Nvalue==null) continue;
                    var rec = dbdata.Where(x => x.Code == item?.Code).FirstOrDefault();
                    if (rec != null)
                    {
                          _ = await _sjcRepo.UpdateMainDataItemValueByIdAsync(rec?.Id, item?.Nvalue??0);
                           nCnt++;
                        
                    }
                }
                return Json(new { msg = $"Бяха заредени {nCnt} записа", success = true });
            }
            else
            {
                return Json(new { msg = $"Липсват данни за предходен период: месец={nm}, год={ny}", success = false });
            }
        }
    }
}
