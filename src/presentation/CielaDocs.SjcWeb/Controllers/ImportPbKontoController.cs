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
    public class ImportPbKontoController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;
       
        private FilterMainDataVm? FilterData = null;

        public ImportPbKontoController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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
        public IActionResult Index()
        {

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
                if ((nYear < 2022) && (nYear > 2040))
                {
                    return Json(new { msg = $"Неразпозната година:{ny} от формата на файла", success = false });
                }
                var court = await _sjcRepo.GetCourtByKontoCodeAsync(kontoCode);
                if (court == null)
                {
                    return Json(new { msg = $"Неоткрит код {kontoCode} на отчетна единица", success = false });
                }
                List<int> yearsLst = new List<int> { 
                    nYear, nYear+1, nYear+2
                };
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
                    decimal nv1 = 0;
                    decimal nv2 = 0;
                    decimal nv3 = 0;

                    while (row <= rowCount)
                    {
                       
                        value1 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                        value2 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                        value3 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 6).GetString();
                        code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                        decimal.TryParse(value1, out nv1);
                        decimal.TryParse(value2, out nv2);
                        decimal.TryParse(value3, out nv3);
                        dic.Add(new DraftBudgetRow { Id = row, Code = code, Value1 = nv1, Value2=nv2, Value3=nv3 });
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
                if ((nMonth < 1) && (nMonth > 12) && (nYear < 2022) && (nYear > 2040))
                {
                    return (0, 0);
                }
                var court = await _sjcRepo.GetCourtByKontoCodeAsync(kontoCode);
                if (court == null)
                {
                    return (0, 0);
                }

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
    }
}
