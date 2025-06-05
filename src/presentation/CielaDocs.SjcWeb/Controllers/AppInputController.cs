using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;

using ClosedXML.Excel;

using DocumentFormat.OpenXml.Drawing.Charts;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CielaDocs.SjcWeb.Controllers
{
    public class AppInputController : Controller
    {
        private readonly ILogger<AppInputController> _logger;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;


        public AppInputController(ILogger<AppInputController> logger, IConfiguration configuration,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo,
                         IWebHostEnvironment env, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _logger = logger;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _env = env;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;
        }
        public async Task<IActionResult> Index()
        {
           var inp = HttpContext.Session.Get<AppInputFilter> ("FilterAppInputSess") ?? new AppInputFilter();
            var court = await _mediator.Send(new GetCourtByIdQuery { Id = inp?.CourtId ?? 0 });
            ViewData["court"] = court;
            ViewBag.Month = inp?.Nmonth;
            ViewBag.Year = inp?.PlannedYear;
            _=await _sjcServiceV2.SpInitAppInputAsync(inp?.CourtId ?? 0, inp?.Nmonth??0, inp?.PlannedYear??0);
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Достъп до входни данни от {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = empl?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetAppInputDataGrid()
        {
            
                var inp = HttpContext.Session.Get<AppInputFilter>("FilterAppInputSess") ?? new AppInputFilter();
                try
                {
                    var data = await _sjcService.QueryRawList<AppInputVm>($@"
                                    SELECT a.Id
                                          ,a.CourtId
                                          ,a.MetricsFieldId
                                          ,a.Nmonth
                                          ,a.PlannedYear
                                          ,a.Nvalue
                                          ,a.EnteredDate
                                     ,m.Code as MetricsFieldCode
                                    ,m.Name as MetricsFieldName
                                    ,c.Name as CourtName
                                    FROM AppInput a
                                    left join MetricsField m on a.MetricsFieldId=m.id
                                    left join Court c on a.courtId=c.Id
                                   
                                    where a.courtId={inp?.CourtId??0} and a.plannedYear={inp?.PlannedYear ?? 0} and a.nmonth={inp?.Nmonth??0} ");
                    return Json(data.ToList());
                }
                catch (Exception ex)
                {
                    return Json(new List<AppInputVm>());
                }

        }

        [HttpGet]
        public PartialViewResult UploadFilePartial() => PartialView("UploadFilePartial");
      
        [HttpPost]
        public async Task<JsonResult> UpdateAppInputDataItem(int key, string values)
        {
            var nv = new Nvalues();
            JsonConvert.PopulateObject(values, nv);
            _ = await _sjcService.ExecuteRawSql($@"UPDATE AppInput SET Nvalue ={nv?.Nvalue??0}, EnteredDate=getDate() WHERE Id = {key}") ;
            return Json(string.Empty);
        }
        [HttpGet]
        public async Task<JsonResult> GetSummarizedAppInputDataGrid(string? id) { 
           
            try
            {
                if(string.IsNullOrWhiteSpace(id)) return Json(new List<AppInputSummarizedVm>());
                string[] args = id.Split('|');

                int.TryParse(args[0], out int institutionTypeId);
                int.TryParse(args[1], out int courtTypeId);
                int.TryParse(args[2], out int courtId);
                int.TryParse(args[3], out int nMonth);
                int.TryParse(args[4], out int nYear);
                if (nYear == 0 || nMonth == 0)
                    return Json(new List<AppInputSummarizedVm>());
                var sqlBuilder = new StringBuilder(@"
                                    SELECT a.Id
                                          ,a.CourtId
                                          ,a.MetricsFieldId
                                          ,a.Nmonth
                                          ,a.PlannedYear
                                          ,SUM(a.Nvalue) OVER (
                                              PARTITION BY a.CourtId, a.MetricsFieldId, a.PlannedYear, a.Nmonth
                                              ORDER BY a.MetricsFieldId
                                              ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                                          ) AS CalculatedValue
                                          ,a.EnteredDate
                                          ,m.Code AS MetricsFieldCode
                                          ,m.Name AS MetricsFieldName
                                          ,c.Name AS CourtName
                                    FROM AppInput a
                                    LEFT JOIN MetricsField m ON a.MetricsFieldId = m.Id
                                    LEFT JOIN Court c ON a.CourtId = c.Id
                                    WHERE a.PlannedYear = @nYear AND a.Nmonth = @nMonth
                                ");
                var parameters = new Dictionary<string, object>
                {
                    { "@nYear", nYear },
                    { "@nMonth", nMonth }
                };

                // Add filtering conditions
                if (courtId > 0)
                {
                    sqlBuilder.Append(" AND a.CourtId = @courtId");
                    parameters.Add("@courtId", courtId);
                }
                else if (courtTypeId > 0)
                {
                    sqlBuilder.Append(" AND a.CourtId IN (SELECT Id FROM Court WHERE CourtTypeId = @courtTypeId)");
                    parameters.Add("@courtTypeId", courtTypeId);
                }
                else if (institutionTypeId > 0)
                {
                    sqlBuilder.Append(@"
                 AND a.CourtId IN (
                SELECT Id FROM Court
                WHERE CourtTypeId IN (
                    SELECT Id FROM CourtType WHERE InstitutionTypeId = @institutionTypeId
                )
            )
        ");
                    parameters.Add("@institutionTypeId", institutionTypeId);
                }

                sqlBuilder.Append(" ORDER BY a.MetricsFieldId");

                var sql = sqlBuilder.ToString();
                var data = await _sjcService.QueryRawList<AppInputSummarizedVm>(sql, parameters);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<AppInputVm>());
            }
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> LoadCustomAppInputExcelFile(string id, int? courtId, int? ny, int? nm)
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
           
            string sFileNameOnly = System.IO.Path.GetFileNameWithoutExtension(id);
            int nCnt = 0;
            StringBuilder sb = new StringBuilder();
         
                try
                {

                    
                   var  dic = new Dictionary<string, decimal>();
                using (var excelWorkbook = new XLWorkbook(file))
                    {
                        var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                        var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                        var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                        int row = 2;
                        string code = string.Empty;
                        string value1 = string.Empty;

                        decimal nv1 = 0;


                        while (row <= rowCount)
                        {
                        code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                        value1 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                         
                            decimal.TryParse(value1, out nv1);
                           
                            if (!string.IsNullOrEmpty(code))
                            {
                                dic.Add(Regex.Replace(code, @"\s+", ""), nv1);
                            }
                            row++;
                        }
                    }
                int nMetricsFieldId = 0;
                foreach (var item in dic)
                    {
                        if (item.Value == 0) continue;
                    nMetricsFieldId = await _sjcService.QueryRaw<int?>($"SELECT id FROM MetricsField WHERE code='{item.Key}'")??0;
                    int foundId= await _sjcService.QueryRaw<int?>($"SELECT id FROM AppInput WHERE courtId={courtId??0} and MetricsFieldId={nMetricsFieldId} and Nmonth={nm} and PlannedYear={ny}") ?? 0;
                    if (foundId == 0) {
                        _ = await _sjcService.ExecuteRawSql($@"INSERT INTO [dbo].[AppInput]([CourtId],[MetricsFieldId],[Nmonth],[PlannedYear],[Nvalue],[EnteredDate])
                                VALUES({courtId ?? 0},{nMetricsFieldId},{nm},{ny},{item.Value},getDate())");
                    }
                    else
                    {
                        _ = await _sjcService.ExecuteRawSql($@"Update [dbo].[AppInput] set Nvalue={item.Value},EnteredDate=getDate() WHERE Id={foundId}");
                        
                    }
                   nCnt++;
                }


                //-------------------------------------------------------------------------------
                return Json(new { msg = $"Бяха заредени данни за {nCnt} записа", success = true });


                }
                catch (Exception ex)
                {
                    return Json(new { msg = "Грешка при четене на файл: " + ex?.Message, success = false });
                }
            


            return Json(new { msg = "", success = false });
        }
    }
}
