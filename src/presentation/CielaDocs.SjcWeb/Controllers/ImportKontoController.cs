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
    public class ImportKontoController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;

        private FilterMainDataVm? FilterData = null;

        public ImportKontoController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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
        public async Task<JsonResult> LoadCustomExcelFile(string id, bool? isOverwrite)
        {
            try
            {
          
                // Check the File is received

                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { msg = "Невалиден файл за зареждане на данни", success = false });

                string file = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
                var supportedTypes = new[] {"xlsm","xls", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(id).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    return Json(new { msg = "Невалиден файл за зареждане на данни. Задължително изберете файл с разширение .xlsm,.xlsx или .xls", success = false });
                }
                string sFileNameOnly = System.IO.Path.GetFileNameWithoutExtension(id);
                string[] par=sFileNameOnly.Split('_');
                string nm = par[2].Substring(0, 2); 
                string ny= par[2].Substring(2, 2);

                string kontoCode = par[3];
                int.TryParse(nm, out int nMonth);
                int.TryParse("20" + ny, out int nYear);
                if ((nMonth < 1) && (nMonth > 12) && (nYear < 2022) && (nYear > 2040))
                {
                    return Json(new { msg = $"Неразпознат месец: {nm} от формата на файла или година:{ny}", success = false });
                }
                var court = await _sjcRepo.GetCourtByKontoCodeAsync(kontoCode);
                if (court == null) {
                    return Json(new { msg = $"Неоткрит код {kontoCode} на отчетна единица", success = false });
                }

                List<KontoRow> dic = new();
                using (var excelWorkbook = new XLWorkbook(file))
                {
                    var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                    var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                    var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                    int row = 12;
                    string code=string.Empty;
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
                var courtData = await _sjcRepo.GetProgramDataCourtByCourtIdAsync(court?.Id,nYear);
                int nCnt = 0;

                foreach (var row in courtData)
                {
                    var kCodes = await _sjcRepo.GetKontoCodesFromProgramDef(row?.FunctionalSubAreaId, row?.RowNum);
                    if (string.IsNullOrWhiteSpace(kCodes)) continue;
                    decimal? nval = 0;
                    var KontoCodesList=kCodes.Split(',');
                    foreach (var kCode in KontoCodesList) { 
                        var foundItem=dic.Where(x=>x.Code==kCode).ToList();
                        if (!foundItem.Any()) continue;
                       nval+=foundItem.Sum(x => x.Value);
                    }
                    KontoMonthDataVm dataVm = new KontoMonthDataVm()
                    {
                        CourtId = court?.Id,
                        ProgramDefId = 0,
                        FunctionalSubAreaId = row?.FunctionalSubAreaId ?? 0,
                        RowNum = row?.RowNum,
                        RowCode = row?.RowCode??string.Empty,
                        NMonth = nMonth,
                        NYear = nYear,
                        Nvalue = nval ?? 0,
                        CurrencyId = 0,
                        CurrencyMeasureId = 0,
                        Datum = DateTime.Now,
                    };
                    _ = await _sjcRepo.AddUpdateKontoMonthData(dataVm);
                    _ = await _sjcRepo.ProgramDataCourtAsync(court?.Id, row?.FunctionalSubAreaId ?? 0, row?.RowNum,nYear);
                    nCnt++;
                }
              
                return Json(new { msg = $"Бяха заредени данни за {nCnt} записа", success = true });


            }
            catch (Exception ex)
            {
                return Json(new { msg = "Грешка при четене на файл: " + ex?.Message, success = false });
            }
        }
        public async Task<JsonResult> LoadFromFolderKontoFile() {
            try
            {
                int nCnt=0;int fileCnt = 0;
                string[] filePaths = System.IO.Directory.GetFiles(System.IO.Path.Combine(_env.WebRootPath + "/uploads/"));
                foreach (string filePath in filePaths)
                {

                   var res = await LoadKontoFile(System.IO.Path.GetFileName(filePath));
                    fileCnt += res.Item1;
                    nCnt += res.Item2;
                }
                return Json(new { msg = $"Процедурата по зареждане на месечни данни от Конто приключи. Файлове с данни {fileCnt}, заредени записи: {nCnt}", success = true });
            }
            catch (Exception ex )
            {
                return Json(new { msg = "Грешка при импорт на файлове: " + ex?.Message, success = false });
            }
        }
        private async Task<(int,int)> LoadKontoFile(string fileName)
        {
            try
            {

                // Check the File is received

                if (string.IsNullOrWhiteSpace(fileName))
                    return (0, 0);

                string file = System.IO.Path.Combine(_env.WebRootPath + "/uploads/", fileName);
                var supportedTypes = new[] { "xlsm", "xls", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(fileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    return (0,0);
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

                return (1,nCnt);


            }
            catch (Exception ex)
            {
                return (0, 0);
            }
        }
    }
}
