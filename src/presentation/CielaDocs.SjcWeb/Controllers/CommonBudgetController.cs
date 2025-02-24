using CielaDocs.Application;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using ClosedXML.Excel;

using DevExpress.XtraRichEdit.Fields;

using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Office;

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
            
            int.TryParse(par, out int nYear);
            
            ViewBag.Nyear = nYear;
            @ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);
            var ret = await GetExcelFileByName("CommonBudget.xlsx", nYear,currencyId??0,ViewBag.Currency);
            ViewBag.Success = ret.Item1.ToString();
            ViewBag.ResultFile=ret.Item2.ToString();
                //string excelFile = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", fl);
            return View();

        }
        private async Task<(Boolean,string)> GetExcelFileByName(string templateName,  int ny,int currencyId, string currencyCode)
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

                excelWorkbook.Worksheets.Worksheet(1).Cell("B4").SetValue($"{ny} г.( в {currencyCode})");
                excelWorkbook.Worksheets.Worksheet(1).Cell("B46").SetValue($"{ny + 1} г.( в {currencyCode})");
                excelWorkbook.Worksheets.Worksheet(1).Cell("B88").SetValue($" {ny + 2} г.( в {currencyCode})");
                excelWorkbook.Worksheets.Worksheet(1).Cell("B129").SetValue($"{ny + 3} г.( в {currencyCode})");

                for (int i = 1; i < 9; i++)
                {
                        switch (i) {
                        case 1:
                            {
                                var prog1 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(1, ny,currencyId);

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C16").SetValue($"{prog1.Where(x=>x.RowNum==10).Sum(x=>x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D16").SetValue($"{prog1.Where(x=>x.RowNum==10).Sum(x=>x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E16").SetValue($"{prog1.Where(x => x.RowNum == 10).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F16").SetValue($"{prog1.Where(x => x.RowNum == 10).Sum(x => x.Nval4)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("C17").SetValue($"{prog1.Where(x => x.RowNum == 11).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D17").SetValue($"{prog1.Where(x => x.RowNum == 11).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E17").SetValue($"{prog1.Where(x => x.RowNum == 11).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F17").SetValue($"{prog1.Where(x => x.RowNum == 11).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C19").SetValue($"{prog1.Where(x => x.RowNum == 13).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D19").SetValue($"{prog1.Where(x => x.RowNum == 13).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E19").SetValue($"{prog1.Where(x => x.RowNum == 13).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F19").SetValue($"{prog1.Where(x => x.RowNum == 13).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C20").SetValue($"{prog1.Where(x => x.RowNum == 14).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D20").SetValue($"{prog1.Where(x => x.RowNum == 14).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E20").SetValue($"{prog1.Where(x => x.RowNum == 14).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F20").SetValue($"{prog1.Where(x => x.RowNum == 14).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C22").SetValue($"{prog1.Where(x => x.RowNum == 16).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D22").SetValue($"{prog1.Where(x => x.RowNum == 16).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E22").SetValue($"{prog1.Where(x => x.RowNum == 16).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F22").SetValue($"{prog1.Where(x => x.RowNum == 16).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C24").SetValue($"{prog1.Where(x => x.RowNum == 18).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D24").SetValue($"{prog1.Where(x => x.RowNum == 18).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E24").SetValue($"{prog1.Where(x => x.RowNum == 18).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F24").SetValue($"{prog1.Where(x => x.RowNum == 18).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C48").SetValue($"{prog1.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D48").SetValue($"{prog1.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E48").SetValue($"{prog1.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F48").SetValue($"{prog1.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C49").SetValue($"{prog1.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D49").SetValue($"{prog1.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E49").SetValue($"{prog1.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F49").SetValue($"{prog1.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C51").SetValue($"{prog1.Where(x => x.RowNum == 36).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D51").SetValue($"{prog1.Where(x => x.RowNum == 36).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E51").SetValue($"{prog1.Where(x => x.RowNum == 36).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F51").SetValue($"{prog1.Where(x => x.RowNum == 36).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(2).Cell("C52").SetValue($"{prog1.Where(x => x.RowNum == 37).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("D52").SetValue($"{prog1.Where(x => x.RowNum == 37).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("E52").SetValue($"{prog1.Where(x => x.RowNum == 37).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(2).Cell("F52").SetValue($"{prog1.Where(x => x.RowNum == 37).Sum(x => x.Nval4)}");


                            }
                                break;
                        case 2: {
                                var prog2 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(2, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(3).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C16").SetValue($"{prog2.Where(x => x.RowNum == 10).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D16").SetValue($"{prog2.Where(x => x.RowNum == 10).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E16").SetValue($"{prog2.Where(x => x.RowNum == 10).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F16").SetValue($"{prog2.Where(x => x.RowNum == 10).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C17").SetValue($"{prog2.Where(x => x.RowNum == 11).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D17").SetValue($"{prog2.Where(x => x.RowNum == 11).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E17").SetValue($"{prog2.Where(x => x.RowNum == 11).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F17").SetValue($"{prog2.Where(x => x.RowNum == 11).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C19").SetValue($"{prog2.Where(x => x.RowNum == 13).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D19").SetValue($"{prog2.Where(x => x.RowNum == 13).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E19").SetValue($"{prog2.Where(x => x.RowNum == 13).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F19").SetValue($"{prog2.Where(x => x.RowNum == 13).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C20").SetValue($"{prog2.Where(x => x.RowNum == 14).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D20").SetValue($"{prog2.Where(x => x.RowNum == 14).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E20").SetValue($"{prog2.Where(x => x.RowNum == 14).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F20").SetValue($"{prog2.Where(x => x.RowNum == 14).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C22").SetValue($"{prog2.Where(x => x.RowNum == 16).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D22").SetValue($"{prog2.Where(x => x.RowNum == 16).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E22").SetValue($"{prog2.Where(x => x.RowNum == 16).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F22").SetValue($"{prog2.Where(x => x.RowNum == 16).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C46").SetValue($"{prog2.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D46").SetValue($"{prog2.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E46").SetValue($"{prog2.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F46").SetValue($"{prog2.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C47").SetValue($"{prog2.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D47").SetValue($"{prog2.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E47").SetValue($"{prog2.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F47").SetValue($"{prog2.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C49").SetValue($"{prog2.Where(x => x.RowNum == 36).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D49").SetValue($"{prog2.Where(x => x.RowNum == 36).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E49").SetValue($"{prog2.Where(x => x.RowNum == 36).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F49").SetValue($"{prog2.Where(x => x.RowNum == 36).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(3).Cell("C50").SetValue($"{prog2.Where(x => x.RowNum == 37).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("D50").SetValue($"{prog2.Where(x => x.RowNum == 37).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("E50").SetValue($"{prog2.Where(x => x.RowNum == 37).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(3).Cell("F50").SetValue($"{prog2.Where(x => x.RowNum == 37).Sum(x => x.Nval4)}");
                            }
                            break;
                        case 3: {
                                var prog3 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(3, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(4).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C16").SetValue($"{prog3.Where(x => x.RowNum == 10).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D16").SetValue($"{prog3.Where(x => x.RowNum == 10).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E16").SetValue($"{prog3.Where(x => x.RowNum == 10).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F16").SetValue($"{prog3.Where(x => x.RowNum == 10).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C17").SetValue($"{prog3.Where(x => x.RowNum == 11).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D17").SetValue($"{prog3.Where(x => x.RowNum == 11).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E17").SetValue($"{prog3.Where(x => x.RowNum == 11).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F17").SetValue($"{prog3.Where(x => x.RowNum == 11).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C19").SetValue($"{prog3.Where(x => x.RowNum == 13).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D19").SetValue($"{prog3.Where(x => x.RowNum == 13).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E19").SetValue($"{prog3.Where(x => x.RowNum == 13).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F19").SetValue($"{prog3.Where(x => x.RowNum == 13).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C20").SetValue($"{prog3.Where(x => x.RowNum == 14).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D20").SetValue($"{prog3.Where(x => x.RowNum == 14).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E20").SetValue($"{prog3.Where(x => x.RowNum == 14).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F20").SetValue($"{prog3.Where(x => x.RowNum == 14).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C22").SetValue($"{prog3.Where(x => x.RowNum == 16).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D22").SetValue($"{prog3.Where(x => x.RowNum == 16).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E22").SetValue($"{prog3.Where(x => x.RowNum == 16).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F22").SetValue($"{prog3.Where(x => x.RowNum == 16).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C24").SetValue($"{prog3.Where(x => x.RowNum == 18).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D24").SetValue($"{prog3.Where(x => x.RowNum == 18).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E24").SetValue($"{prog3.Where(x => x.RowNum == 18).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F24").SetValue($"{prog3.Where(x => x.RowNum == 18).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C25").SetValue($"{prog3.Where(x => x.RowNum == 19).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D25").SetValue($"{prog3.Where(x => x.RowNum == 19).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E25").SetValue($"{prog3.Where(x => x.RowNum == 19).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F25").SetValue($"{prog3.Where(x => x.RowNum == 19).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C26").SetValue($"{prog3.Where(x => x.RowNum == 20).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D26").SetValue($"{prog3.Where(x => x.RowNum == 20).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E26").SetValue($"{prog3.Where(x => x.RowNum == 20).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F26").SetValue($"{prog3.Where(x => x.RowNum == 20).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C50").SetValue($"{prog3.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D50").SetValue($"{prog3.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E50").SetValue($"{prog3.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F50").SetValue($"{prog3.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C51").SetValue($"{prog3.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D51").SetValue($"{prog3.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E51").SetValue($"{prog3.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F51").SetValue($"{prog3.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C53").SetValue($"{prog3.Where(x => x.RowNum == 36).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D53").SetValue($"{prog3.Where(x => x.RowNum == 36).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E53").SetValue($"{prog3.Where(x => x.RowNum == 36).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F53").SetValue($"{prog3.Where(x => x.RowNum == 36).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(4).Cell("C54").SetValue($"{prog3.Where(x => x.RowNum == 37).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("D54").SetValue($"{prog3.Where(x => x.RowNum == 37).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("E54").SetValue($"{prog3.Where(x => x.RowNum == 37).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(4).Cell("F54").SetValue($"{prog3.Where(x => x.RowNum == 37).Sum(x => x.Nval4)}");
                            }
                            break;
                        case 4: {
                                var prog4 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(4, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(5).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C16").SetValue($"{prog4.Where(x => x.RowNum == 10).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D16").SetValue($"{prog4.Where(x => x.RowNum == 10).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E16").SetValue($"{prog4.Where(x => x.RowNum == 10).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F16").SetValue($"{prog4.Where(x => x.RowNum == 10).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C17").SetValue($"{prog4.Where(x => x.RowNum == 11).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D17").SetValue($"{prog4.Where(x => x.RowNum == 11).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E17").SetValue($"{prog4.Where(x => x.RowNum == 11).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F17").SetValue($"{prog4.Where(x => x.RowNum == 11).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C19").SetValue($"{prog4.Where(x => x.RowNum == 13).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D19").SetValue($"{prog4.Where(x => x.RowNum == 13).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E19").SetValue($"{prog4.Where(x => x.RowNum == 13).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F19").SetValue($"{prog4.Where(x => x.RowNum == 13).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C20").SetValue($"{prog4.Where(x => x.RowNum == 14).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D20").SetValue($"{prog4.Where(x => x.RowNum == 14).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E20").SetValue($"{prog4.Where(x => x.RowNum == 14).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F20").SetValue($"{prog4.Where(x => x.RowNum == 14).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C22").SetValue($"{prog4.Where(x => x.RowNum == 16).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D22").SetValue($"{prog4.Where(x => x.RowNum == 16).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E22").SetValue($"{prog4.Where(x => x.RowNum == 16).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F22").SetValue($"{prog4.Where(x => x.RowNum == 16).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C24").SetValue($"{prog4.Where(x => x.RowNum == 18).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D24").SetValue($"{prog4.Where(x => x.RowNum == 18).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E24").SetValue($"{prog4.Where(x => x.RowNum == 18).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F24").SetValue($"{prog4.Where(x => x.RowNum == 18).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C25").SetValue($"{prog4.Where(x => x.RowNum == 19).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D25").SetValue($"{prog4.Where(x => x.RowNum == 19).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E25").SetValue($"{prog4.Where(x => x.RowNum == 19).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F25").SetValue($"{prog4.Where(x => x.RowNum == 19).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C26").SetValue($"{prog4.Where(x => x.RowNum == 20).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D26").SetValue($"{prog4.Where(x => x.RowNum == 20).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E26").SetValue($"{prog4.Where(x => x.RowNum == 20).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F26").SetValue($"{prog4.Where(x => x.RowNum == 20).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C50").SetValue($"{prog4.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D50").SetValue($"{prog4.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E50").SetValue($"{prog4.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F50").SetValue($"{prog4.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C51").SetValue($"{prog4.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D51").SetValue($"{prog4.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E51").SetValue($"{prog4.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F51").SetValue($"{prog4.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C53").SetValue($"{prog4.Where(x => x.RowNum == 36).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D53").SetValue($"{prog4.Where(x => x.RowNum == 36).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E53").SetValue($"{prog4.Where(x => x.RowNum == 36).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F53").SetValue($"{prog4.Where(x => x.RowNum == 36).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(5).Cell("C54").SetValue($"{prog4.Where(x => x.RowNum == 37).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("D54").SetValue($"{prog4.Where(x => x.RowNum == 37).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("E54").SetValue($"{prog4.Where(x => x.RowNum == 37).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(5).Cell("F54").SetValue($"{prog4.Where(x => x.RowNum == 37).Sum(x => x.Nval4)}");
                            } 
                            break;
                        case 5: {
                                var prog5 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(5, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(6).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C16").SetValue($"{prog5.Where(x => x.RowNum == 10).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D16").SetValue($"{prog5.Where(x => x.RowNum == 10).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E16").SetValue($"{prog5.Where(x => x.RowNum == 10).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F16").SetValue($"{prog5.Where(x => x.RowNum == 10).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C17").SetValue($"{prog5.Where(x => x.RowNum == 11).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D17").SetValue($"{prog5.Where(x => x.RowNum == 11).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E17").SetValue($"{prog5.Where(x => x.RowNum == 11).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F17").SetValue($"{prog5.Where(x => x.RowNum == 11).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C19").SetValue($"{prog5.Where(x => x.RowNum == 13).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D19").SetValue($"{prog5.Where(x => x.RowNum == 13).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E19").SetValue($"{prog5.Where(x => x.RowNum == 13).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F19").SetValue($"{prog5.Where(x => x.RowNum == 13).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C20").SetValue($"{prog5.Where(x => x.RowNum == 14).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D20").SetValue($"{prog5.Where(x => x.RowNum == 14).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E20").SetValue($"{prog5.Where(x => x.RowNum == 14).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F20").SetValue($"{prog5.Where(x => x.RowNum == 14).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C22").SetValue($"{prog5.Where(x => x.RowNum == 16).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D22").SetValue($"{prog5.Where(x => x.RowNum == 16).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E22").SetValue($"{prog5.Where(x => x.RowNum == 16).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F22").SetValue($"{prog5.Where(x => x.RowNum == 16).Sum(x => x.Nval4)}");

                              
                                excelWorkbook.Worksheets.Worksheet(6).Cell("C46").SetValue($"{prog5.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D46").SetValue($"{prog5.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E46").SetValue($"{prog5.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F46").SetValue($"{prog5.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C47").SetValue($"{prog5.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D47").SetValue($"{prog5.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E47").SetValue($"{prog5.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F47").SetValue($"{prog5.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C49").SetValue($"{prog5.Where(x => x.RowNum == 36).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D49").SetValue($"{prog5.Where(x => x.RowNum == 36).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E49").SetValue($"{prog5.Where(x => x.RowNum == 36).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F49").SetValue($"{prog5.Where(x => x.RowNum == 36).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(6).Cell("C50").SetValue($"{prog5.Where(x => x.RowNum == 37).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("D50").SetValue($"{prog5.Where(x => x.RowNum == 37).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("E50").SetValue($"{prog5.Where(x => x.RowNum == 37).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(6).Cell("F50").SetValue($"{prog5.Where(x => x.RowNum == 37).Sum(x => x.Nval4)}");
                            }
                            break;
                        case 6: {
                                var prog6 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(6, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(7).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C16").SetValue($"{prog6.Where(x => x.RowNum == 10).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D16").SetValue($"{prog6.Where(x => x.RowNum == 10).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E16").SetValue($"{prog6.Where(x => x.RowNum == 10).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F16").SetValue($"{prog6.Where(x => x.RowNum == 10).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C17").SetValue($"{prog6.Where(x => x.RowNum == 11).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D17").SetValue($"{prog6.Where(x => x.RowNum == 11).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E17").SetValue($"{prog6.Where(x => x.RowNum == 11).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F17").SetValue($"{prog6.Where(x => x.RowNum == 11).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C19").SetValue($"{prog6.Where(x => x.RowNum == 13).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D19").SetValue($"{prog6.Where(x => x.RowNum == 13).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E19").SetValue($"{prog6.Where(x => x.RowNum == 13).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F19").SetValue($"{prog6.Where(x => x.RowNum == 13).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C20").SetValue($"{prog6.Where(x => x.RowNum == 14).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D20").SetValue($"{prog6.Where(x => x.RowNum == 14).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E20").SetValue($"{prog6.Where(x => x.RowNum == 14).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F20").SetValue($"{prog6.Where(x => x.RowNum == 14).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C22").SetValue($"{prog6.Where(x => x.RowNum == 16).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D22").SetValue($"{prog6.Where(x => x.RowNum == 16).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E22").SetValue($"{prog6.Where(x => x.RowNum == 16).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F22").SetValue($"{prog6.Where(x => x.RowNum == 16).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C24").SetValue($"{prog6.Where(x => x.RowNum == 18).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D24").SetValue($"{prog6.Where(x => x.RowNum == 18).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E24").SetValue($"{prog6.Where(x => x.RowNum == 18).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F24").SetValue($"{prog6.Where(x => x.RowNum == 18).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C25").SetValue($"{prog6.Where(x => x.RowNum == 19).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D25").SetValue($"{prog6.Where(x => x.RowNum == 19).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E25").SetValue($"{prog6.Where(x => x.RowNum == 19).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F25").SetValue($"{prog6.Where(x => x.RowNum == 19).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C26").SetValue($"{prog6.Where(x => x.RowNum == 20).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D26").SetValue($"{prog6.Where(x => x.RowNum == 20).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E26").SetValue($"{prog6.Where(x => x.RowNum == 20).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F26").SetValue($"{prog6.Where(x => x.RowNum == 20).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C50").SetValue($"{prog6.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D50").SetValue($"{prog6.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E50").SetValue($"{prog6.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F50").SetValue($"{prog6.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C51").SetValue($"{prog6.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D51").SetValue($"{prog6.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E51").SetValue($"{prog6.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F51").SetValue($"{prog6.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C53").SetValue($"{prog6.Where(x => x.RowNum == 36).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D53").SetValue($"{prog6.Where(x => x.RowNum == 36).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E53").SetValue($"{prog6.Where(x => x.RowNum == 36).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F53").SetValue($"{prog6.Where(x => x.RowNum == 36).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(7).Cell("C54").SetValue($"{prog6.Where(x => x.RowNum == 37).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("D54").SetValue($"{prog6.Where(x => x.RowNum == 37).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("E54").SetValue($"{prog6.Where(x => x.RowNum == 37).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(7).Cell("F54").SetValue($"{prog6.Where(x => x.RowNum == 37).Sum(x => x.Nval4)}");
                            }
                            break;
                        case 7: {
                                var prog7 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(7, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(8).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(8).Cell("C12").SetValue($"{prog7.Where(x => x.RowNum == 6).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("D12").SetValue($"{prog7.Where(x => x.RowNum == 6).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("E12").SetValue($"{prog7.Where(x => x.RowNum == 6).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("F12").SetValue($"{prog7.Where(x => x.RowNum == 6).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(8).Cell("C13").SetValue($"{prog7.Where(x => x.RowNum == 7).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("D13").SetValue($"{prog7.Where(x => x.RowNum == 7).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("E13").SetValue($"{prog7.Where(x => x.RowNum == 7).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(8).Cell("F13").SetValue($"{prog7.Where(x => x.RowNum == 7).Sum(x => x.Nval4)}");
                            }
                            break;
                        case 8: {
                                var prog8 = await _sjcRepo.GetProgramDataCourt3YCommonCurrencyAsync(8, ny, currencyId);
                                excelWorkbook.Worksheets.Worksheet(9).Cell("C3").SetValue($"{ny} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("D3").SetValue($"{ny + 1} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("E3").SetValue($" {ny + 2} г.( в {currencyCode})");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("F3").SetValue($"{ny + 3} г.( в {currencyCode})");

                                excelWorkbook.Worksheets.Worksheet(9).Cell("C11").SetValue($"{prog8.Where(x => x.RowNum == 6).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("D11").SetValue($"{prog8.Where(x => x.RowNum == 6).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("E11").SetValue($"{prog8.Where(x => x.RowNum == 6).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("F11").SetValue($"{prog8.Where(x => x.RowNum == 6).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(9).Cell("C12").SetValue($"{prog8.Where(x => x.RowNum == 7).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("D12").SetValue($"{prog8.Where(x => x.RowNum == 7).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("E12").SetValue($"{prog8.Where(x => x.RowNum == 7).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("F12").SetValue($"{prog8.Where(x => x.RowNum == 7).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(9).Cell("C13").SetValue($"{prog8.Where(x => x.RowNum == 8).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("D13").SetValue($"{prog8.Where(x => x.RowNum == 8).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("E13").SetValue($"{prog8.Where(x => x.RowNum == 8).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("F13").SetValue($"{prog8.Where(x => x.RowNum == 8).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(9).Cell("C38").SetValue($"{prog8.Where(x => x.RowNum == 33).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("D38").SetValue($"{prog8.Where(x => x.RowNum == 33).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("E38").SetValue($"{prog8.Where(x => x.RowNum == 33).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("F38").SetValue($"{prog8.Where(x => x.RowNum == 33).Sum(x => x.Nval4)}");

                                excelWorkbook.Worksheets.Worksheet(9).Cell("C39").SetValue($"{prog8.Where(x => x.RowNum == 34).Sum(x => x.Nval1)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("D39").SetValue($"{prog8.Where(x => x.RowNum == 34).Sum(x => x.Nval2)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("E39").SetValue($"{prog8.Where(x => x.RowNum == 34).Sum(x => x.Nval3)}");
                                excelWorkbook.Worksheets.Worksheet(9).Cell("F39").SetValue($"{prog8.Where(x => x.RowNum == 34).Sum(x => x.Nval4)}");
                            }
                            break;
                        }
                
                }
                    excelWorkbook.SaveAs(excelResultFilePath);

               

            }

            return (true, resultFile);

        }
    }
}
