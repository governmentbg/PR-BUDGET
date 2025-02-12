using CielaDocs.Application;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using ClosedXML.Excel;

using DevExpress.XtraRichEdit.Fields;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Controllers
{
    [Authorize]
    public class CommonBudgetController : Controller
    {
        private readonly ILogger<HomeController> _logger;



        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;


        public CommonBudgetController(ILogger<HomeController> logger, IConfiguration configuration, ISendGridMailer emailSender,
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


        public async Task<IActionResult> Index(string par, int? currencyId)
        {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int nMonth1);
            int.TryParse(args[1], out int nMonth2);
            int.TryParse(args[2], out int nYear);
            
            ViewBag.Nyear = nYear;
            ViewBag.Nmonth1 = nMonth1;
            ViewBag.Nmonth2 = nMonth2;
            @ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            var ret = await GetExcelFileByName("CommonBudget.xlsx", nYear);
            ViewBag.Success = ret.Item1.ToString();
            ViewBag.Message=ret.Item2.ToString();
                //string excelFile = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", fl);
            return View();

        }
        private async Task<(Boolean,string)> GetExcelFileByName(string templateName,  int ny)
        {

            string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
            string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
            string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/{templateName}");

            

            using (var excelWorkbook = new XLWorkbook(excelFile))
            {

                int sheetCount = excelWorkbook.Worksheets.Count;
                if (sheetCount !=9) {
                    return (false, "Очаква се файлът да съдържа 9 шиита!");
                }

                //excelWorkbook.Worksheets.Worksheet(1).Cell("A1").SetValue("РАЗЧЕТИ");
                //excelWorkbook.Worksheets.Worksheet(1).Cell("A2").SetValue($"по проектобюджета/тригодишните бюджетни прогнози за периода {ny}-{ny + 2} г.");
                //excelWorkbook.Worksheets.Worksheet(1).Cell("B4").SetValue(court?.Name ?? string.Empty);
                //excelWorkbook.Worksheets.Worksheet(1).Cell("G4").SetValue(court?.KontoCode ?? string.Empty);
                //excelWorkbook.Worksheets.Worksheet(1).Cell("D7").SetValue($"Бюджетна прогноза за {ny} г.");
                //excelWorkbook.Worksheets.Worksheet(1).Cell("E7").SetValue($"Бюджетна прогноза за {ny + 1} г.");
                //excelWorkbook.Worksheets.Worksheet(1).Cell("F7").SetValue($"Бюджетна прогноза за {ny + 2} г.");

                excelWorkbook.SaveAs(excelResultFilePath);

            }

            return (true, excelResultFilePath);

        }
    }
}
