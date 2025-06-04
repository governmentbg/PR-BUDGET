using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;

using DocumentFormat.OpenXml.Drawing.Charts;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Data;
using System.Text;
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
    }
}
