using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.ExpressionEngine;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;

using ClosedXML.Excel;

using DevExpress.Export;

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
using System.Text;
using System.Text.RegularExpressions;

namespace CielaDocs.SjcWeb.Controllers
{
    public class ImportPbKontoController : Controller
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
        private FilterMainDataVm? FilterData = null;

        public ImportPbKontoController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, 
                        ILogRepository logRepo, ISjcBudgetRepository sjcRepo, 
                        IWebHostEnvironment env, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _logger = logger;
            _mediator = mediator;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _env = env;
            _sjcService=sjcService;
            _sjcServiceV2 = sjcServiceV2;
        }
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePeriod"] = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
            ViewData["Cfg"] = await _sjcRepo.GetCfgAsync();
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Достъп до импорт на данни проектобюджет от {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

            return View();
        }




        // [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> LoadCustomExcelFile(string id, bool? isOverwrite,bool? isFirstInit)
        {
           
            try
            {
                string s = string.Empty;
                // Check the File is received
                int nCnt = 0;
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { msg = "Невалиден файл за зареждане на данни", success = false });
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Зареждане на файл {id}  от {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

                string file = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
                var supportedTypes = new[] { "xlsm", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(id).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    return Json(new { msg = "Невалиден файл за зареждане на данни. Задължително изберете файл с разширение .xlsm,.xlsx ", success = false });
                }
                string sFileNameOnly = System.IO.Path.GetFileNameWithoutExtension(id);
                string[] par = sFileNameOnly.Split('_');

                string ny = par[1].Substring(0, 2);

                string kontoCode = par[2];

                int.TryParse("20" + ny, out int nYear);
                if ((nYear < 2022) && (nYear > 2049))
                {
                    return Json(new { msg = $"Неразпозната година:{ny} от формата на файла", success = false });
                }
                var court = await _sjcRepo.GetCourtByKontoCodeAsync(kontoCode);
                if (court == null)
                {
                    return Json(new { msg = $"Неоткрит код {kontoCode} на отчетна единица", success = false });
                }
                List<int> yearsLst = new List<int> { 
                    nYear, nYear+1, nYear+2, nYear+3
                };


                //===========check active period restriction========================
                var actuvePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                if ((nYear < actuvePeriod.Y1) || (nYear > actuvePeriod.Y4))
                {
                    return Json(new { msg = $"Година {nYear} е извън обхвата на активния период! Моля проверете!", success = false });
                }
                //------check locked period------------
                var checkLocked = await _sjcService.QueryRaw<KontoPbCourtLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbCourtLocked a where a.CourtId={court?.Id ?? 0} and a.Nyear={nYear}");
               
                if (checkLocked != null)
                {
                    return Json(new { msg = $"Този период е заключен! Моля проверете!", success = false });
                }
                //-------end check locked period------------------------
                List<DraftBudgetRow> dic = new();
                using (var excelWorkbook = new XLWorkbook(file))
                {
                    var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                    var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                    var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                    int row = 8;
                    string code = string.Empty;
                    string value1 = string.Empty;
                    string value2 = string.Empty;
                    string value3 = string.Empty;
                    string value4 = string.Empty;
                    decimal nv1 = 0;
                    decimal nv2 = 0;
                    decimal nv3 = 0;
                    decimal nv4 = 0;

                    while (row <= rowCount)
                    {
                       
                        value1 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                        value2 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                        value3 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 6).GetString();
                        value4 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                        code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 8).GetString();
                        decimal.TryParse(value1, out nv1);
                        decimal.TryParse(value2, out nv2);
                        decimal.TryParse(value3, out nv3);
                        decimal.TryParse(value4, out nv4);
                        dic.Add(new DraftBudgetRow { Id = row, Code = code, Value1 = nv1, Value2=nv2, Value3=nv3, Value4=nv4 });
                        row++;

                    }
                }

                //list of programs by court
                var courtInPrograms = await _sjcRepo.GetCourtInProgramByCourtIdAsync(court?.Id);
                nCnt = 0;
                if (courtInPrograms.Any()) {
                    foreach (var item in courtInPrograms) {
                        //-------------
                        int nYIndex = 0;
                      
                        foreach (var yearItem in yearsLst)
                        {

                            if (isFirstInit ?? false == true)
                            {
                                _ = await _sjcRepo.FirstInitProgramDataDraftBudgetCourtAsync(item?.CourtId, item?.FunctionalSubAreaId ?? 0, yearItem);
                            }
                            nYIndex++;

                                var programDefCodes = await _sjcRepo.GetProgramDefProgCodesByProgramIdAsync(item?.FunctionalSubAreaId??0);

                                if (programDefCodes.Any()) {

                                
                                        foreach (var prowDef in programDefCodes)
                                        {

                                            var progCode = prowDef?.ProgCode;
                                            if (string.IsNullOrWhiteSpace(progCode)) continue;
                                            decimal? nval = 0;
                                            var dicFiltered = dic.Where(x => x.Code.ContainsWord(progCode));

                                            switch (nYIndex)
                                            {
                                                case 1:
                                                    { nval += dicFiltered.Sum(x => x.Value1); }
                                                    break;
                                                case 2:
                                                    { nval += dicFiltered.Sum(x => x.Value2); }
                                                    break;
                                                case 3:
                                                    { nval += dicFiltered.Sum(x => x.Value3); }
                                                    break;
                                                case 4:
                                                    { nval += dicFiltered.Sum(x => x.Value4); }
                                                    break;

                                    }

                                            s += $"CourtId={item?.CourtId},FunctionalSubAreaId={prowDef?.FunctionalSubAreaId ?? 0},rowNum={prowDef?.RowNum},nYear={yearItem}, nval={nval}, progCode={progCode}" + Environment.NewLine;
                                            _ = await _sjcRepo.ProgramDataDraftBudgetCourtAsync(item?.CourtId, prowDef?.FunctionalSubAreaId ?? 0, prowDef?.RowNum, yearItem, nval);
                                            nCnt++;
                                        }
                                }


                        }
                        //-------------
                    }
                }
               // var z = s;
                return Json(new { msg = $"Бяха заредени данни за {nCnt} записа", success = true });


            }
            catch (Exception ex)
            {
                return Json(new { msg = "Грешка при четене на файл: " + ex?.Message, success = false });
            }
        }
        public async Task<JsonResult> LoadFromFolderKontoFile()
        {
            try
            {
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Зареждане на файлове от папка  от {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

                int nCnt = 0; int fileCnt = 0;
                string[] filePaths = System.IO.Directory.GetFiles(System.IO.Path.Combine(_env.WebRootPath + "/uploads/"));
                foreach (string filePath in filePaths)
                {

                    var res = await LoadKontoFile(System.IO.Path.GetFileName(filePath));
                    fileCnt += res.Item1;
                    nCnt += res.Item2;
                }
                return Json(new { msg = $"Процедурата по зареждане на месечни данни от Конто приключи. Файлове с данни {fileCnt}, заредени записи: {nCnt}", success = true });
            }
            catch (Exception ex)
            {
                return Json(new { msg = "Грешка при импорт на файлове: " + ex?.Message, success = false });
            }
        }
        private async Task<(int, int)> LoadKontoFile(string fileName)
        {
            try
            {

                // Check the File is received

                if (string.IsNullOrWhiteSpace(fileName))
                    return (0, 0);

                string file = System.IO.Path.Combine(_env.WebRootPath + "/uploads/", fileName);
                var supportedTypes = new[] { "xlsm", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(fileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    return (0, 0);
                }
                string sFileNameOnly = System.IO.Path.GetFileNameWithoutExtension(fileName);
                string[] par = sFileNameOnly.Split('_');
                string nm = par[2].Substring(0, 2);
                string ny = par[2].Substring(2, 2);

                string kontoCode = par[3];
                int.TryParse(nm, out int nMonth);
                int.TryParse("20" + ny, out int nYear);
                if ((nMonth < 1) && (nMonth > 12) && (nYear < 2022) && (nYear > 2049))
                {
                    return (0, 0);
                }
                var court = await _sjcRepo.GetCourtByKontoCodeAsync(kontoCode);
                if (court == null)
                {
                    return (0, 0);
                }
                //===========check active period restriction========================
                var actuvePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                if ((nYear < actuvePeriod.Y1) || (nYear > actuvePeriod.Y4))
                {
                    return (0, 0);
                }
                //------check locked period------------
                var checkLocked = await _sjcService.QueryRaw<KontoPbCourtLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbCourtLocked a where a.CourtId={court?.Id ?? 0} and a.Nyear={nYear}");

                if (checkLocked != null)
                {
                    return (0,0);
                }
                //-------end check locked period------------------------
                List<KontoRow> dic = new();
                using (var excelWorkbook = new XLWorkbook(file))
                {
                    var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                    var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                    var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                    int row = 12;
                    string code = string.Empty;
                    string value = string.Empty;

                    while (row <= rowCount)
                    {
                        code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                        value = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (decimal.TryParse(value, out decimal d))
                            {

                                dic.Add(new KontoRow { Id = row, Code = code, Value = d });
                            }
                        }


                        row++;

                    }
                }
                var courtData = await _sjcRepo.GetProgramDataCourtByCourtIdAsync(court?.Id, nYear);
                int nCnt = 0;

                foreach (var row in courtData)
                {
                    var kCodes = await _sjcRepo.GetKontoCodesFromProgramDef(row?.FunctionalSubAreaId, row?.RowNum);
                    if (string.IsNullOrWhiteSpace(kCodes)) continue;
                    decimal? nval = 0;
                    var KontoCodesList = kCodes.Split(',');
                    foreach (var kCode in KontoCodesList)
                    {
                        var foundItem = dic.Where(x => x.Code == kCode).ToList();
                        if (!foundItem.Any()) continue;
                        nval += foundItem.Sum(x => x.Value);
                    }
                    KontoMonthDataVm dataVm = new KontoMonthDataVm()
                    {
                        CourtId = court?.Id,
                        ProgramDefId = 0,
                        FunctionalSubAreaId = row?.FunctionalSubAreaId ?? 0,
                        RowNum = row?.RowNum,
                        RowCode = row?.RowCode,
                        NMonth = nMonth,
                        NYear = nYear,
                        Nvalue = nval ?? 0,
                        CurrencyId = 0,
                        CurrencyMeasureId = 0,
                        Datum = DateTime.Now,
                    };
                    _ = await _sjcRepo.AddUpdateKontoMonthData(dataVm);
                    _ = await _sjcRepo.ProgramDataCourtAsync(court?.Id, row?.FunctionalSubAreaId ?? 0, row?.RowNum, nYear);
                    nCnt++;
                }

                return (1, nCnt);


            }
            catch (Exception ex)
            {
                return (0, 0);
            }
        }
        public ActionResult ImportTypePartial(int? importTypeId)
        {
            switch (importTypeId)
            {
                case -1: return PartialView("_EmptyView");
                case 0: return RedirectToAction("GetImportTypeEntityPartialView", "ImportPbKonto", new { area = "" });
                case 1: return RedirectToAction("GetImportTypeExpertPartialView", "ImportPbKonto", new { area = "" });
                default: return PartialView("_EmptyView");
            }
        }
        public ActionResult GetImportTypeEntityPartialView()
        {
            
            return PartialView("_ImportEntityPartialView");
        }
        public ActionResult GetImportTypeExpertPartialView()
        {

            return PartialView("_ImportExpertPartialView");
        }
        [HttpGet]
        [AllowAnonymous]
        public PartialViewResult AddImportPbKontoLockedPartial()
        {

            return PartialView("AddImportPbKontoLockedPartial");

        }
        [HttpGet]
        public async Task<PartialViewResult> AnalizeDraftBudgetPartial(int? importTypeId, int? functionalSubAreaId,int? institutionTypeId, int? courtTypeId, int? ny)
        {
            ViewBag.FunctionalSubAreaId = functionalSubAreaId ?? 0;
            ViewBag.FunctionalSubAreaName = await _sjcRepo.QueryRawAsync<String>($"Select Name from FunctionalSubArea where id={functionalSubAreaId ?? 0}");
            ViewBag.CourtTypeId = courtTypeId ?? 0;
            ViewBag.CourtTypeName = await _sjcRepo.QueryRawAsync<String>($"Select Name from CourtType where id={courtTypeId ?? 0}");
            ViewBag.InstitutionTypeId = institutionTypeId ?? 0;
            ViewBag.InstitutionTypeIName = await _sjcRepo.QueryRawAsync<String>($"Select Name from InstitutionType where id={institutionTypeId ?? 0}");
            ViewBag.ImportTypeId = importTypeId ?? 0;
            ViewBag.ImportTypeName = (importTypeId == 1) ? "Експертен проектобюджет" : "Проектобюджет на отчетни единици";
            ViewBag.Ny = ny ?? 0;
            return PartialView("AnalizeDraftBudgetPartial");
        }
        [AllowAnonymous]
        public FileResult DownloadDraftBudgetFile(int importType)
        {
            string excelFile = (importType==1)?System.IO.Path.Combine(_env.WebRootPath + "/templates/DraftExpertBudget.xlsx"): System.IO.Path.Combine(_env.WebRootPath + "/templates/DraftBudget.xlsx");
            byte[] data = System.IO.File.ReadAllBytes(excelFile);
            string fileName = $"DraftBudget_{new Random().Next(1, 1000)}.xlsx";
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        private Tuple<string, int> GetExcelFileHeaderByName(string tempFileName)
        {
            string excelFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{tempFileName}");
            int ny = 0;
            string kontoCode = string.Empty;
            using (var excelWorkbook = new XLWorkbook(excelFilePath))
            {
                kontoCode = excelWorkbook.Worksheets.Worksheet(1).Cell("H5").GetString();
                string sy = excelWorkbook.Worksheets.Worksheet(1).Cell("D8").GetString();
                int.TryParse(new String(sy.Where(Char.IsDigit).ToArray()), out ny);
            }
            return new(kontoCode, ny);
        }
        private Tuple<string, int> GetExcelFileInstitutionHeaderByName(string tempFileName)
        {
            string excelFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{tempFileName}");
            int ny = 0;
            string institutionTypeId = string.Empty;
            using (var excelWorkbook = new XLWorkbook(excelFilePath))
            {
                institutionTypeId = excelWorkbook.Worksheets.Worksheet(1).Cell("H5").GetString();
                string sy = excelWorkbook.Worksheets.Worksheet(1).Cell("D8").GetString();
                int.TryParse(new String(sy.Where(Char.IsDigit).ToArray()), out ny);
            }
            return new(institutionTypeId, ny);
        }
        [HttpPost]
        [IgnoreAntiforgeryToken] // Add this to bypass CSRF protection
        public IActionResult CheckBudgetExcelFile(int id, int courtId, int ny, int importType, int institutionTypeId)
        {
            return Json(new { success = true, resultfile = "example.xlsx", msg = "File processed successfully!" });
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> LoadCustomDraftBudgetExcelFile(string id, int? courtId, int? ny, int? importType, int? institutionTypeId)
        {
            string s = string.Empty;

            if (string.IsNullOrWhiteSpace(id)) return Json(new { msg = "Невалиден файл за зареждане на данни", success = false });


            string file = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
            var supportedTypes = new[] { "xlsm", "xlsx" };
            var fileExt = System.IO.Path.GetExtension(id).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                return Json(new { msg = "Невалиден файл за зареждане на данни. Задължително изберете файл с разширение .xlsm,.xlsx ", success = false });
            }
            if ((ny is null) || (ny < 2024))
            {
                return Json(new { msg = "Изберете година преди да прикачите фаила", success = false });
            }
            if (importType is null) { return Json(new { msg = "Изберете вида за отчетна единица или експертен", success = false }); }

            if ((importType == 0) && ((courtId is null) || (courtId < 1)))
            {
                return Json(new { msg = "Неизбрана отчетна единица", success = false });
            }
            if ((importType == 1) && ((institutionTypeId is null) || (institutionTypeId < 1)))
            {
                return Json(new { msg = "Неизбран вид институция за експертен бюджет", success = false });
            }
            string sFileNameOnly = System.IO.Path.GetFileNameWithoutExtension(id);
            int nCnt = 0;
            StringBuilder sb = new StringBuilder();
            if (importType == 0)
            {
                try
                {

                    var excelHeaderData = GetExcelFileHeaderByName(id);

                    int nYear = ny??0;
                    //var court = await _sjcRepo.GetCourtByKontoCodeAsync(excelHeaderData.Item1);
                    //if ((court == null) || (court?.Id < 1))
                    //{
                    //    return Json(new { msg = $"Неоткрит код {excelHeaderData.Item1} на отчетна единица", success = false });
                    //}
                    List<int> yearsLst = new List<int> {
                    nYear, nYear+1, nYear+2, nYear+3
                    };
                    //===========check active period restriction========================
                    var actuvePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                    if ((nYear < actuvePeriod.Y1) || (nYear > actuvePeriod.Y4))
                    {
                        return Json(new { msg = $"Година {nYear} е извън обхвата на активния период! Моля проверете!", success = false });
                    }

                    //------check locked period------------
                    var checkLocked = await _sjcService.QueryRaw<KontoPbCourtLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbCourtLocked a where a.CourtId={courtId ?? 0} and a.Nyear={nYear}");

                    if (checkLocked != null)
                    {
                        return Json(new { msg = $"Този период е заключен! Моля проверете!", success = false });
                    }
                    //-------end check locked period------------------------
                    List<DraftBudgetRow> dic = new();
                    using (var excelWorkbook = new XLWorkbook(file))
                    {
                        var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                        var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                        var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                        int row = 9;
                        string code = string.Empty;
                        string value1 = string.Empty;
                        string value2 = string.Empty;
                        string value3 = string.Empty;
                        string value4 = string.Empty;
                        decimal nv1 = 0;
                        decimal nv2 = 0;
                        decimal nv3 = 0;
                        decimal nv4 = 0;

                        while (row <= rowCount)
                        {

                            value1 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                            value2 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                            value3 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 6).GetString();
                            value4 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                            code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 8).GetString();
                            decimal.TryParse(value1, out nv1);
                            decimal.TryParse(value2, out nv2);
                            decimal.TryParse(value3, out nv3);
                            decimal.TryParse(value4, out nv4);
                            if (!string.IsNullOrEmpty(code))
                            {
                                dic.Add(new DraftBudgetRow { Id = row, Code = Regex.Replace(code, @"\s+", ""), Value1 = nv1, Value2 = nv2, Value3 = nv3, Value4 = nv4 });
                            }
                            row++;

                        }
                    }
                    var r = s;

                    //list of programs by court
                    var courtInPrograms = await _sjcRepo.GetCourtInProgramByCourtIdAsync(courtId);
                    nCnt = 0;
                    if (courtInPrograms.Any())
                    {
                        foreach (var item in courtInPrograms)
                        {
                            //-------------
                            int nYIndex = 0;

                            foreach (var yearItem in yearsLst)
                            {

                                //always do this nullify values
                                _ = await _sjcRepo.FirstInitProgramDataDraftBudgetCourtAsync(item?.CourtId, item?.FunctionalSubAreaId ?? 0, yearItem);

                                nYIndex++;

                                var programDefCodes = await _sjcRepo.GetProgramDefProgCodesByProgramIdAsync(item?.FunctionalSubAreaId ?? 0);

                                if (programDefCodes.Any())
                                {

                                    foreach (var prowDef in programDefCodes)
                                    {

                                        if (prowDef?.ProgCode is null) continue;
                                        var progCode = Regex.Replace(prowDef.ProgCode, @"\s+", "");
                                        if (string.IsNullOrWhiteSpace(progCode)) continue;
                                        decimal? nval = 0;

                                        var dicFiltered = dic.Where(x => x.Code.ContainsWord(progCode)).ToList();

                                        switch (nYIndex)
                                        {
                                            case 1:
                                                { nval += dicFiltered.Sum(x => x.Value1); }
                                                break;
                                            case 2:
                                                { nval += dicFiltered.Sum(x => x.Value2); }
                                                break;
                                            case 3:
                                                { nval += dicFiltered.Sum(x => x.Value3); }
                                                break;
                                            case 4:
                                                { nval += dicFiltered.Sum(x => x.Value4); }
                                                break;

                                        }
                                        sb.AppendLine($"CourtId:{item?.CourtId}, FunctionalSubAreaId:{prowDef?.FunctionalSubAreaId ?? 0} rowNum={prowDef?.RowNum} year={yearItem} val={nval}, progCode='{progCode}'");
                                        _ = await _sjcRepo.ProgramDataDraftBudgetCourtAsync(item?.CourtId, prowDef?.FunctionalSubAreaId ?? 0, prowDef?.RowNum, yearItem, nval);
                                        nCnt++;
                                    }
                                }
                                _ = await _sjcRepo.sp_RecalculateProgramDataCourtAsync(item?.FunctionalSubAreaId, yearItem, item?.CourtId);
                            }
                            //-------------
                        }
                    }

                    //------------------------------------------------------------------------------
                    var resultfile = $"import_{Guid.NewGuid().ToString("N")}.txt";
                    string resultfilepath = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", resultfile);
                    using (StreamWriter writer = new StreamWriter(resultfilepath, false, Encoding.UTF8))
                    {
                        writer.Write(sb.ToString());
                    }
                    //-------------------------------------------------------------------------------
                    return Json(new { msg = $"Бяха заредени данни за {nCnt} записа", success = true, resultfile = resultfile });


                }
                catch (Exception ex)
                {
                    return Json(new { msg = "Грешка при четене на файл: " + ex?.Message, success = false });
                }
            }
            else if (importType == 1)
            {

                try
                {

                    var excelHeaderData = GetExcelFileInstitutionHeaderByName(id);

                    int nYear = ny??0;
                    var inst = await _sjcRepo.GetInstitutionTypeByIdAsync(institutionTypeId);
                    if (inst == null)
                    {
                        return Json(new { msg = $"Неоткрит код {excelHeaderData.Item1} на отчетна единица", success = false });
                    }


                    if ((nYear < 2022) && (nYear > 2050))
                    {
                        return Json(new { msg = $"Неразпозната година:{nYear} от данните във файла", success = false });
                    }

                    List<int> yearsLst = new List<int> {
                    nYear, nYear+1, nYear+2, nYear+3
                    };
                    //===========check active period restriction========================
                    var actuvePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                    if ((nYear < actuvePeriod.Y1) || (nYear > actuvePeriod.Y4))
                    {
                        return Json(new { msg = $"Година {nYear} е извън обхвата на активния период! Моля проверете!", success = false });
                    }
                    //------check locked period------------
                    var checkLocked = await _sjcService.QueryRaw<KontoPbInstitutionTypeLockedVm>($@"SELECT TOP 1 a.Id,a.InstitutionTypeId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbInstitutionTypeLocked a where a.InstitutionTypeId={inst?.Id ?? 0} and a.Nyear={nYear}");

                    if (checkLocked != null)
                    {
                        return Json(new { msg = $"Този период е заключен! Моля проверете!", success = false });
                    }
                    //-------end check locked period------------------------
                    List<DraftBudgetRow> dic = new();
                    using (var excelWorkbook = new XLWorkbook(file))
                    {
                        var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                        var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                        var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                        int row = 9;
                        string code = string.Empty;
                        string value1 = string.Empty;
                        string value2 = string.Empty;
                        string value3 = string.Empty;
                        string value4 = string.Empty;
                        decimal nv1 = 0;
                        decimal nv2 = 0;
                        decimal nv3 = 0;
                        decimal nv4 = 0;
                        while (row <= rowCount)
                        {

                            value1 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                            value2 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                            value3 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 6).GetString();
                            value4 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                            code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 8).GetString();
                            decimal.TryParse(value1, out nv1);
                            decimal.TryParse(value2, out nv2);
                            decimal.TryParse(value3, out nv3);
                            decimal.TryParse(value4, out nv4);
                            if (!string.IsNullOrEmpty(code))
                            {
                                dic.Add(new DraftBudgetRow { Id = row, Code = Regex.Replace(code, @"\s+", ""), Value1 = nv1, Value2 = nv2, Value3 = nv3, Value4 = nv4 });
                            }
                            row++;

                        }
                    }

                    //list of programs by court
                    var instInPrograms = await _sjcRepo.GetInstitutionInProgramByInstitutionTypeIdAsync(inst?.Id);
                    nCnt = 0;
                    if (instInPrograms.Any())
                    {
                        foreach (var item in instInPrograms)
                        {
                            //-------------
                            int nYIndex = 0;

                            foreach (var yearItem in yearsLst)
                            {

                                //always do this nullify values
                                _ = await _sjcRepo.FirstInitProgramDataDraftBudgetInstitutionAsync(item?.InstitutionTypeId, item?.FunctionalSubAreaId ?? 0, yearItem);

                                nYIndex++;

                                var programDefCodes = await _sjcRepo.GetProgramDefProgCodesByProgramIdAsync(item?.FunctionalSubAreaId ?? 0);

                                if (programDefCodes.Any())
                                {

                                    foreach (var prowDef in programDefCodes)
                                    {
                                        if(prowDef?.ProgCode is null) continue;
                                        var progCode = Regex.Replace(prowDef.ProgCode, @"\s+", ""); 
                                        if (string.IsNullOrWhiteSpace(progCode)) continue;
                                        decimal? nval = 0;
                                        var dicFiltered = dic.Where(x => x.Code.ContainsWord(progCode)).ToList();
                                        //if (progCode == "03.01.04.K") {
                                        //    var dg= dic.Select(x => x.Code).ToList();
                                        //}
                                        switch (nYIndex)
                                        {
                                            case 1:
                                                { nval += dicFiltered.Sum(x => x.Value1); }
                                                break;
                                            case 2:
                                                { nval += dicFiltered.Sum(x => x.Value2); }
                                                break;
                                            case 3:
                                                { nval += dicFiltered.Sum(x => x.Value3); }
                                                break;
                                            case 4:
                                                { nval += dicFiltered.Sum(x => x.Value4); }
                                                break;

                                        }
                                        sb.AppendLine($"ItemTypeId:{item?.InstitutionTypeId}, FunctionalSubAreaId:{prowDef?.FunctionalSubAreaId ?? 0} rowNum={prowDef?.RowNum} year={yearItem} val={nval}, progCode='{progCode}'");
                                        _ = await _sjcRepo.ProgramDataDraftBudgetInstitutionAsync(item?.InstitutionTypeId, prowDef?.FunctionalSubAreaId ?? 0, prowDef?.RowNum, yearItem, nval);
                                        nCnt++;
                                    }
                                }
                                _ = await _sjcRepo.sp_RecalculateProgramDataInstitutionAsync(item?.FunctionalSubAreaId, yearItem, item?.InstitutionTypeId);
                            }
                            //-------------
                        }
                    }

                    // var z = s;
                    //------------------------------------------------------------------------------
                    var resultfile = $"import_{Guid.NewGuid().ToString("N")}.txt";
                    string resultfilepath = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", resultfile);
                    using (StreamWriter writer = new StreamWriter(resultfilepath, false, Encoding.UTF8))
                    {
                        writer.Write(sb.ToString());
                    }
                    //-------------------------------------------------------------------------------
                    return Json(new { msg = $"Бяха заредени данни за {nCnt} записа", success = true, resultfile = resultfile });


                }
                catch (Exception ex)
                {
                    return Json(new { msg = "Грешка при четене на файл: " + ex?.Message, success = false });
                }
            }

            return Json(new { msg = "", success = false });
        }
    }
}
