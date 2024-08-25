using CielaDocs.SjcWeb.Models;


using System;
using System.IO;
using System.Linq;
using System.Text;
using DevExpress.AspNetCore.Spreadsheet;
using DevExpress.Spreadsheet;
using DevExpress.XtraSpreadsheet.Export;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using CielaDocs.Application.Models;
using ClosedXML.Excel;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.Shared.Repository;
using DevExpress.XtraReports.Parameters.Native;
using DocumentFormat.OpenXml.Bibliography;
using DevExtreme.AspNet.Mvc;
using Microsoft.Graph;
using CielaDocs.Domain.Entities;
using CielaDocs.SjcWeb.ViewModels;
using DevExpress.XtraSpreadsheet.API.Native.Implementation;
using Microsoft.AspNetCore.Http;
using MediatR;
using DevExpress.XtraPrinting.Native;
using CielaDocs.Application;
using Microsoft.AspNetCore.Authorization;



namespace CielaDocs.SjcWeb.Areas.CourtUser.Controllers
{
    [Area("CourtUser")]
    [Authorize(Policy = "CourtUserOnly")]
    [IgnoreAntiforgeryToken]
    public class ExcelViewerController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly ILogRepository _logRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private Dictionary<string, string> repl_values = new Dictionary<string, string>();
        private static bool addToProgramData = false;
        public ExcelViewerController(IWebHostEnvironment env, ISjcBudgetRepository sjcRepo, ILogRepository logRepo, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _env = env;
            _sjcRepo = sjcRepo;
            _logRepo = logRepo;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }
        public static Stream GetDocumentContentStream(string file)
        {
            return new MemoryStream(System.IO.File.ReadAllBytes(file));
        }

        public async Task<IActionResult> Index(string id, string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Отваряне на файл {id}  от {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

                if (fileExtension.EndsWith("xlsx") || fileExtension.EndsWith("xlsm"))
                {
                    Func<Stream> contentAccessorByStream = () => GetDocumentContentStream(filePath);
                    var viewmodel = new SpreadsheetDocumentContentFromStream(Path.GetFileName(filePath), contentAccessorByStream);
                    ViewBag.Import = string.Empty;
                    ViewBag.ImportType = 0;
                    ViewBag.FileName = Path.GetFileName(filePath);
                    return View("Index", viewmodel);
                }


            }
            string[] par = id.Split('|');
            if (!string.IsNullOrWhiteSpace(par[0]))
            {
                string excelFile = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
                Func<Stream> contentAccessorByStream = () => GetDocumentContentStream(excelFile);
                var viewmodel = new SpreadsheetDocumentContentFromStream(id, contentAccessorByStream);
                ViewBag.Import = string.Empty;
                ViewBag.ImportType = 0;
                ViewBag.FileName = id;
                return View("Index", viewmodel);
            }
            else
            {
                if (par[4] == "0")
                {
                    int.TryParse(par[1], out int courtId);
                    int.TryParse(par[2], out int ny);

                    bool.TryParse(par[4], out addToProgramData);
                    string fl = await GetExcelFileByName("DraftBudget.xlsx", courtId, ny);

                    string excelFile = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", fl);

                    Func<Stream> contentAccessorByStream = () => GetDocumentContentStream(excelFile);
                    var viewmodel = new SpreadsheetDocumentContentFromStream(fl, contentAccessorByStream);
                    ViewBag.ImportType = 0;
                    ViewBag.FileName = fl;
                    return View("Index", viewmodel);
                }
                else if (par[4] == "1")
                {
                    int.TryParse(par[5], out int institutionTypeId);
                    int.TryParse(par[2], out int ny);

                    bool.TryParse(par[4], out addToProgramData);
                    string fl = await GetExpertExcelFileByName("DraftExpertBudget.xlsx", institutionTypeId, ny);

                    string excelFile = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", fl);

                    Func<Stream> contentAccessorByStream = () => GetDocumentContentStream(excelFile);
                    var viewmodel = new SpreadsheetDocumentContentFromStream(fl, contentAccessorByStream);
                    ViewBag.ImportType = 1;
                    ViewBag.FileName = fl;
                    return View("Index", viewmodel);
                }
                else return View("Index", null);
            }
        }
        private async Task<string> GetExcelFileByName(string templateName, int courtId, int ny)
        {

            string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
            string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
            string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/{templateName}");

            var court = await _sjcRepo.GetCourtByIdAsync(courtId);

            using (var excelWorkbook = new XLWorkbook(excelFile))
            {

                excelWorkbook.Worksheets.Worksheet(1).Cell("A1").SetValue("РАЗЧЕТИ");
                excelWorkbook.Worksheets.Worksheet(1).Cell("A2").SetValue($"по проектобюджета/тригодишните бюджетни прогнози за периода {ny}-{ny + 2} г.");
                excelWorkbook.Worksheets.Worksheet(1).Cell("B4").SetValue(court?.Name ?? string.Empty);
                excelWorkbook.Worksheets.Worksheet(1).Cell("G4").SetValue(court?.KontoCode ?? string.Empty);
                excelWorkbook.Worksheets.Worksheet(1).Cell("D7").SetValue($"Бюджетна прогноза за {ny} г.");
                excelWorkbook.Worksheets.Worksheet(1).Cell("E7").SetValue($"Бюджетна прогноза за {ny + 1} г.");
                excelWorkbook.Worksheets.Worksheet(1).Cell("F7").SetValue($"Бюджетна прогноза за {ny + 2} г.");

                excelWorkbook.SaveAs(excelResultFilePath);

            }

            return resultFile;

        }
        private async Task<string> GetExpertExcelFileByName(string templateName, int? institutionTypeId, int ny)
        {

            string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
            string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
            string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/{templateName}");

            var court = await _sjcRepo.GetInstitutionTypeByIdAsync(institutionTypeId ?? 0);

            using (var excelWorkbook = new XLWorkbook(excelFile))
            {

                excelWorkbook.Worksheets.Worksheet(1).Cell("A1").SetValue("РАЗЧЕТИ");
                excelWorkbook.Worksheets.Worksheet(1).Cell("A2").SetValue($"по проектобюджета/тригодишните бюджетни прогнози за периода {ny}-{ny + 2} г.");
                excelWorkbook.Worksheets.Worksheet(1).Cell("B4").SetValue(court?.Name ?? string.Empty);
                excelWorkbook.Worksheets.Worksheet(1).Cell("G4").SetValue(court?.Id.ToString());
                excelWorkbook.Worksheets.Worksheet(1).Cell("D7").SetValue($"Бюджетна прогноза за {ny} г.");
                excelWorkbook.Worksheets.Worksheet(1).Cell("E7").SetValue($"Бюджетна прогноза за {ny + 1} г.");
                excelWorkbook.Worksheets.Worksheet(1).Cell("F7").SetValue($"Бюджетна прогноза за {ny + 2} г.");

                excelWorkbook.SaveAs(excelResultFilePath);

            }

            return resultFile;

        }
        private Tuple<string, int> GetExcelFileHeaderByName(string tempFileName)
        {
            string excelFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{tempFileName}");
            int ny = 0;
            string kontoCode = string.Empty;
            using (var excelWorkbook = new XLWorkbook(excelFilePath))
            {
                kontoCode = excelWorkbook.Worksheets.Worksheet(1).Cell("G4").GetString();
                string sy = excelWorkbook.Worksheets.Worksheet(1).Cell("D7").GetString();
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
                institutionTypeId = excelWorkbook.Worksheets.Worksheet(1).Cell("G4").GetString();
                string sy = excelWorkbook.Worksheets.Worksheet(1).Cell("D7").GetString();
                int.TryParse(new String(sy.Where(Char.IsDigit).ToArray()), out ny);
            }
            return new(institutionTypeId, ny);
        }
        [HttpPost]
        [HttpGet]
        public IActionResult DxDocRequest()
        {
            return SpreadsheetRequestProcessor.GetResponse(HttpContext);
        }
        public IActionResult DownloadXlsx(SpreadsheetClientState spreadsheetState)
        {
            string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var resultFileName = $"{Guid.NewGuid().ToString("N")}.xlsx";
            var spreadsheet = SpreadsheetRequestProcessor.GetSpreadsheetFromState(spreadsheetState);

            MemoryStream stream = new MemoryStream();
            spreadsheet.SaveCopy(stream, DevExpress.Spreadsheet.DocumentFormat.Xlsx);
            stream.Position = 0;
            return File(stream, XlsxContentType, resultFileName);
        }
        public IActionResult DownloadHtml(SpreadsheetClientState spreadsheetState)
        {
            var spreadsheet = SpreadsheetRequestProcessor.GetSpreadsheetFromState(spreadsheetState);

            HtmlDocumentExporterOptions options = new HtmlDocumentExporterOptions();
            options.CssPropertiesExportType = DevExpress.XtraSpreadsheet.Export.Html.CssPropertiesExportType.Style;
            options.Encoding = Encoding.UTF8;
            options.EmbedImages = true;
            options.SheetIndex = spreadsheet.Document.Worksheets.ActiveWorksheet.Index;

            MemoryStream stream = new MemoryStream();
            spreadsheet.Document.ExportToHtml(stream, options);
            stream.Position = 0;
            var resultFileName = $"{Guid.NewGuid().ToString("N")}.html";
            return File(stream, "text/html", resultFileName);
        }
        [HttpPost]
        public async Task<JsonResult> LoadExcelFileToTable(SpreadsheetClientState spreadsheetState, int? importType, string? fileName)
        {

            var id = $"{Guid.NewGuid().ToString("N")}{fileName}";
            string file = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
            var spreadsheet = SpreadsheetRequestProcessor.GetSpreadsheetFromState(spreadsheetState);

            spreadsheet.SaveCopy(file);
            if (importType == 0)
            {
                try
                {
                    string s = string.Empty;
                    // Check the File is received
                    int nCnt = 0;
                    if (string.IsNullOrWhiteSpace(id))
                        return Json(new { msg = "Невалиден файл за зареждане на данни", success = false });


                    var supportedTypes = new[] { "xlsm", "xlsx" };
                    var fileExt = System.IO.Path.GetExtension(id).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        return Json(new { msg = "Невалиден файл за зареждане на данни. Задължително изберете файл с разширение .xlsm,.xlsx ", success = false });
                    }
                    string sFileNameOnly = System.IO.Path.GetFileNameWithoutExtension(id);
                    var excelHeaderData = GetExcelFileHeaderByName(fileName);

                    int nYear = (int)excelHeaderData.Item2;




                    if ((nYear < 2022) && (nYear > 2040))
                    {
                        return Json(new { msg = $"Неразпозната година:{nYear} от данните във файла", success = false });
                    }
                    var court = await _sjcRepo.GetCourtByKontoCodeAsync(excelHeaderData.Item1);
                    if ((court == null) || (court?.Id < 1))
                    {
                        return Json(new { msg = $"Неоткрит код {excelHeaderData.Item1} на отчетна единица", success = false });
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
                            if (!string.IsNullOrEmpty(code))
                            {
                                dic.Add(new DraftBudgetRow { Id = row, Code = code, Value1 = nv1, Value2 = nv2, Value3 = nv3 });
                                // s += $"Id={row},Code={code},values1={nv1},values2={nv2}, value3={nv3}" + Environment.NewLine;
                            }
                            row++;

                        }
                    }


                    //list of programs by court
                    var courtInPrograms = await _sjcRepo.GetCourtInProgramByCourtIdAsync(court?.Id);
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

                                        var progCode = prowDef?.ProgCode;
                                        if (string.IsNullOrWhiteSpace(progCode)) continue;
                                        decimal? nval = 0;
                                        //test only
                                        //if (progCode == "01.01.03.K") {
                                        //    var dicTest = dic.Where(x => x.Code.ContainsWord(progCode)).ToList();
                                        //}
                                        //end test
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

                                        }

                                        // s += $"CourtId={item?.CourtId},FunctionalSubAreaId={prowDef?.FunctionalSubAreaId ?? 0},rowNum={prowDef?.RowNum},nYear={yearItem}, nval={nval}, progCode={progCode}" + Environment.NewLine;
                                        _ = await _sjcRepo.ProgramDataDraftBudgetCourtAsync(item?.CourtId, prowDef?.FunctionalSubAreaId ?? 0, prowDef?.RowNum, yearItem, nval);
                                        nCnt++;
                                    }
                                }
                                _ = await _sjcRepo.sp_RecalculateProgramDataCourtAsync(item?.FunctionalSubAreaId, yearItem, item?.CourtId);
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

            return Json(new { msg = "", success = true });
        }

    }
}
