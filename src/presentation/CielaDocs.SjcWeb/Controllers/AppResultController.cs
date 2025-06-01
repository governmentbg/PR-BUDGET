using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;

using ClosedXML.Excel;

using DevExtreme.AspNet.Mvc;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Controllers
{
    public class AppResultController : Controller
    {
        private readonly ILogger<AppResultController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;
        private const string templateName = "AppResultTemplate.xlsx";
        public AppResultController(ILogger<AppResultController> logger, IWebHostEnvironment env, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _logger= logger;
            _env = env;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;

        }
        private async Task<(string AppName, string ProgramName)> GetProgramNameByAppId(int? appId) { 
            var app = await _sjcService.QueryRaw<AppVm>("SELECT * FROM App WHERE Id=@Id", new { Id = appId });
        
            if (app is null || app?.Id == 0)
            {
                return (AppName: "", ProgramName: "");
            }
            int functionalSubAreaId = app.Id switch
            {
                int n when (n >= 1 && n <= 2) => 1,
                int n when (n >= 3 && n <= 4) => 2,
                int n when (n >= 5 && n <= 6) => 3,
                int n when (n >= 7 && n <= 8) => 4,
                int n when (n >= 9 && n <= 10) => 5,
                int n when (n >= 11 && n <= 12) => 6,
                int n when (n >= 13 && n <= 14) => 7,
                int n when (n >= 15 && n <= 16) => 8,
                _ => 0
            };
            string programName = await _sjcService.QueryRaw<string>("SELECT Name FROM FunctionalSubArea WHERE Id=@Id", new { Id = functionalSubAreaId });
            return (AppName: app.Name ?? "", ProgramName: programName ?? "");

        }
        private async Task<IEnumerable<AppInputSummarizedVm>> GetSummarizedAppInput(int appId,int nMonth, int nYear)
        {
            string sql = $@"SELECT a.Id
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
                    AND a.CourtId IN (SELECT Id FROM Court WHERE CourtTypeId IN (
                SELECT Id FROM CourtType WHERE InstitutionTypeId IN(select InstitutionTypeId from AppRequired where AppId = @appId)
                               
                    )
                 )
                ORDER BY a.MetricsFieldId";
                var parameters = new Dictionary<string, object>
                {
                    { "@appId", appId },
                    { "@nYear", nYear },
                    { "@nMonth", nMonth }
                };
            return  await _sjcService.QueryRawList<AppInputSummarizedVm>(sql, parameters);
        }
        private async Task<List<AppDefVm>> GetAppDefaultByAppId(int? appId) {
            try
            {
                var data = await _sjcService.QueryRawList<AppDefVm>($@"SELECT a.Id ,a.FunctionalSubAreaId ,a.AppId ,a.RowNum ,a.RowCode,a.Name,a.ParentRowNum,a.IsActive ,a.MeasureId,a.Formula,b.Name as AppName,c.Name as MeasureName
          FROM  dbo.AppDef a
         left join App b on a.appId=b.id
         left join Measure c on a.MeasureID=c.id
         where a.AppId={appId ?? 0}");
                return data.ToList();
            }
            catch (Exception ex)
            {
                return new List<AppDefVm>();
            }
        }
        public async Task<IActionResult> Index(string? par)
        {
            string[] args = par.Split('|');
            int.TryParse(args[0], out int appId);
            int.TryParse(args[1], out int nMonth);
            int.TryParse(args[2], out int nYear);
            string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
            string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
            string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/{templateName}");
            var (appName, programName) = await GetProgramNameByAppId(appId);
            var appRequired = await _sjcService.QueryRawList<AppRequiredVm>($@"SELECT a.Id
                                  ,a.AppId
                                  ,a.InstitutionTypeId
                                  ,a.IsActive
	                              ,i.Name as InstitutionTypeName
                                   FROM AppRequired a
                                   left join InstitutionType i on a.InstitutionTypeId=i.id
                                   where a.AppId={appId}");
            var summarizedAppInput = await GetSummarizedAppInput(appId, nMonth, nYear);
            var appDef = await GetAppDefaultByAppId(appId);
            using (var excelWorkbook = new XLWorkbook(excelFile))
            {
                var worksheet = excelWorkbook.Worksheet(1);
                worksheet.Cell("B2").SetValue(programName.Replace("\r\n", ""));
                worksheet.Cell("E1").SetValue(appName);
                worksheet.Cell("B3").Value=$"Период: месец {nMonth} година {nYear}";
              
                int rowOffset = 5;
                for (int i = 0; i < appDef.Count; i++)
                {
                    AppDefVm item = appDef[i];
                    rowOffset+=1;
                    worksheet.Cell("B" + rowOffset).Value = item.RowCode;
                    worksheet.Cell("C" + rowOffset).Value = item.Name;
                    worksheet.Cell("D" + rowOffset).Value = item.MeasureName;
                    worksheet.Cell("E" + rowOffset).Value = 0;
                }
                excelWorkbook.SaveAs(excelResultFilePath);
            }
            ViewBag.ResultFile = resultFile;
            return View();
        }
    }
}
