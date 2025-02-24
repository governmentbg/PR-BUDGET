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
using CielaDocs.Shared.Services;
namespace CielaDocs.SjcWeb.Controllers
{
    [IgnoreAntiforgeryToken]
    public class ExcelReportController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly ILogRepository _logRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;
        private Dictionary<string, string> repl_values = new Dictionary<string, string>();
        private static bool addToProgramData = false;
        public ExcelReportController(IWebHostEnvironment env, ISjcBudgetRepository sjcRepo, ILogRepository logRepo,
            IHttpContextAccessor httpContextAccessor, IMediator mediator, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _env = env;
            _sjcRepo = sjcRepo;
            _logRepo = logRepo;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;
        }
        public static Stream GetDocumentContentStream(string file)
        {
            return new MemoryStream(System.IO.File.ReadAllBytes(file));
        }

        public async Task<IActionResult> Index(string id, string filePath)
        {
            
                var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
            string fullPath= System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{filePath}");

            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Отваряне на файл {id}  от {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });

            
                    Func<Stream> contentAccessorByStream = () => GetDocumentContentStream(fullPath);
                    var viewmodel = new SpreadsheetDocumentContentFromStream(Path.GetFileName(fullPath), contentAccessorByStream);
                    ViewBag.Import = string.Empty;
                    ViewBag.ImportType = 0;
                    ViewBag.FileName = Path.GetFileName(fullPath);
                   
               

            return View("Index", viewmodel);


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
       

    }
}
