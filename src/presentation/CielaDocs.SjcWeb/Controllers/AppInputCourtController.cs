using CielaDocs.Application.Models;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;

using ClosedXML.Excel;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using System.Text;

using System.Text.RegularExpressions;

namespace CielaDocs.SjcWeb.Controllers
{
    public class AppInputCourtController : Controller
    {
        private readonly ILogger<AppInputCommonController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;
        private const int VssCreator = 4;
        public AppInputCourtController(ILogger<AppInputCommonController> logger, IWebHostEnvironment env, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _logger = logger;
            _env = env;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;

        }
        public async Task<IActionResult> IndexAsync()
        {
            var filterData = HttpContext.Session.Get<FilterAppInputCommonVm>("FilterAppInputCommonSess") ?? new FilterAppInputCommonVm();
            var app = await _sjcService.QueryRaw<AppVm>("SELECT * FROM App WHERE Id=@Id", new { Id = filterData.AppId });
            var activeYears = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
            
            int[] years = new int[] { activeYears?.Y1 ?? 0, activeYears?.Y2 ?? 0, activeYears?.Y3 ?? 0, activeYears?.Y4 ?? 0 };
            for (int i = 0; i < years.Length; i++)
            {
                if (years[i] > 0)
                {
                    _ = await _sjcServiceV2.Sp_InitAppInputCourtAsync(filterData?.AppId, filterData?.CourtId,years[i]);
                    await Task.Delay(250);
                }
            }
            ViewBag.AppId = filterData?.AppId;
            ViewBag.CourtId = filterData?.CourtId;
            ViewBag.Year = filterData?.Nyear;
            ViewBag.CourtName= await _sjcService.QueryRaw<string>("SELECT Name FROM Court WHERE Id=@Id", new { Id = filterData?.CourtId });
            ViewBag.AppName = app?.Name ?? string.Empty;
            ViewBag.IsLocked = filterData?.IsLocked ?? false;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetAppInputDataGrid()
        {

            var inp = HttpContext.Session.Get<FilterAppInputCourtVm>("FilterAppInputCommonSess") ?? new FilterAppInputCourtVm();
            string sql = $@"select a.Id,a.AppId,a.CourtId,a.MetricsFieldId,a.PlannedYear,a.EnteredDate, m.Code as MetricsFieldCode,m.Name as MetricsFieldName,a.Nvalue as nval1,b.nvalue as nval2,c.Nvalue as nval3,d.Nvalue as nval4
                                   from AppInputCourt a
                                    left join AppInputCourt b on a.AppId=b.AppId and a.CourtId=b.CourtId and a.MetricsFieldId=b.MetricsFieldId and b.PlannedYear=a.PlannedYear+1
                                    left join AppInputCourt c on a.AppId=c.AppId and a.CourtId=c.CourtId and a.MetricsFieldId=c.MetricsFieldId and c.PlannedYear=a.PlannedYear+2
                                    left join AppInputCourt d on a.AppId=d.AppId and a.CourtId=d.CourtId and a.MetricsFieldId=d.MetricsFieldId and d.PlannedYear=a.PlannedYear+3
                                    left join MetricsField m on a.MetricsFieldId=m.id
                                 AND m.Id IN (
                                     SELECT DISTINCT admf.MetricsFieldId
                                     FROM AppDefMetricsField admf
                                     WHERE admf.AppDefId IN (
                                         SELECT ad.Id FROM AppDef ad WHERE ad.AppId ={inp?.AppId ?? 0}
                                     )
                                 )
                                    where a.id>0 and a.AppId={inp?.AppId ?? 0} and a.CourtId={inp?.CourtId} and a.plannedYear={inp?.Nyear ?? 0} and m.code is not null
                                GROUP BY 
                                    a.Id,
                                    a.AppId,
                                    a.CourtId,
                                    a.MetricsFieldId,
                                    a.PlannedYear,
	                                a.EnteredDate,
                                    m.Code,
                                    m.Name,
	                                a.Nvalue,
	                                b.Nvalue,
	                                c.Nvalue,
	                                d.Nvalue
                                ORDER BY 
                                    a.Id,
                                    a.AppId,
                                    a.CourtId,
                                    m.Code";
            try
            {
                var data = await _sjcService.QueryRawList<AppInputCourt3YVm>(sql);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<AppInputCourt3YVm>());
            }

        }

        [HttpPost]
        public async Task<JsonResult> UpdateAppInputDataItem(int key, string values)
        {
            dynamic objval = Newtonsoft.Json.JsonConvert.DeserializeObject(values);
            var dtype1 = objval.GetType();
            decimal n = 0;
            string name = string.Empty;
            if (objval.GetType() == typeof(JObject))
            {
                foreach (var oelem in objval)
                {
                    name = oelem.Name;
                    decimal.TryParse(oelem.Value.ToString(), out n);
                }
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                // _ = await _sjcRepo.UpdateProgramDataValueByIdAsync(key,name, n);

                _ = await UpdateAppInputCourtData3YValueByIdAsync(key, name, n);



            }
            return Json(string.Empty);
        }
        private async Task<int> UpdateAppInputCourtData3YValueByIdAsync(int? id, string fieldName, decimal? val)
        {
            var activePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
            int currentYear = activePeriod?.Y1 ?? 0;
            var rec = await _sjcService.QueryRaw<AppInputCourtMinVm>("SELECT * FROM AppInputCourt WHERE Id=@Id", new { Id = id });
            switch (fieldName.ToLower())
            {
                case "nval2": currentYear = currentYear + 1; break;
                case "nval3": currentYear = currentYear + 2; break;
                case "nval4": currentYear = currentYear + 3; break;
            }
            var sql = $@"UPDATE AppInputCourt SET Nvalue = {val}, EnteredDate=getDate() WHERE AppId={rec?.AppId ?? 0} and CourtId={rec?.CourtId} and MetricsFieldId={rec?.MetricsFieldId ?? 0} and PlannedYear={currentYear}";
            _ = await _sjcService.ExecuteRawSql(sql);
            return 1;
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> LoadCustomAppInputExcelFile(string id,int? appId, int? courtId, int? ny)
        {
            string s = string.Empty;
            var activePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
            int currentYear = activePeriod?.Y1 ?? 0;
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


                var dic = new List<AppInputCourt3YVm>();
                using (var excelWorkbook = new XLWorkbook(file))
                {
                    var nonEmptyDataRows = excelWorkbook.Worksheet(1).RowsUsed();
                    var rowCount = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
                    var columnCount = excelWorkbook.Worksheet(1).LastColumnUsed().ColumnNumber();
                    int row = 2;
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
                        code = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                        value1 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                        value2 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                        value3 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                        value4 = excelWorkbook.Worksheets.Worksheet(1).Cell(row, 6).GetString();

                        decimal.TryParse(value1, out nv1);
                        decimal.TryParse(value2, out nv2);
                        decimal.TryParse(value3, out nv3);
                        decimal.TryParse(value4, out nv4);
                        if (!string.IsNullOrEmpty(code))
                        {
                            dic.Add(new AppInputCourt3YVm {AppId=appId,CourtId=courtId,MetricsFieldCode=Regex.Replace(code, @"\s+", ""), Nval1=nv1, Nval2=nv2, Nval3=nv3, Nval4=nv4});
                        }
                        row++;
                    }
                }
                int nMetricsFieldId = 0;
                foreach (var item in dic)
                {
                    
                    nMetricsFieldId = await _sjcService.QueryRaw<int?>($"SELECT top 1 id FROM MetricsField WHERE code='{item.MetricsFieldCode}'") ?? 0;
                    int foundId1 = await _sjcService.QueryRaw<int?>($"SELECT top 1 id FROM AppInputCourt WHERE AppId={item.AppId} and courtId={courtId ?? 0} and MetricsFieldId={nMetricsFieldId} and PlannedYear={currentYear}") ?? 0;
                    if (foundId1 == 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"INSERT INTO [dbo].[AppInputCourt]([AppId],[CourtId],[MetricsFieldId],[PlannedYear],[Nvalue],[EnteredDate])
                                VALUES({appId??0},{courtId ?? 0},{nMetricsFieldId},{currentYear},{item.Nval1},getDate())");
                    }
                    else if(foundId1>0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"Update [dbo].[AppInputCourt] set Nvalue={item.Nval1},EnteredDate=getDate() WHERE Id={foundId1}");

                    }
                    int foundId2 = await _sjcService.QueryRaw<int?>($"SELECT top 1 id FROM AppInputCourt WHERE AppId={item.AppId} and courtId={courtId ?? 0} and MetricsFieldId={nMetricsFieldId} and PlannedYear={currentYear + 1}") ?? 0;
                    if (foundId2 == 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"INSERT INTO [dbo].[AppInputCourt]([AppId],[CourtId],[MetricsFieldId],[PlannedYear],[Nvalue],[EnteredDate])
                                VALUES({appId ?? 0},{courtId ?? 0},{nMetricsFieldId},{currentYear + 1},{item.Nval2},getDate())");
                    }
                    else if (foundId2 > 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"Update [dbo].[AppInputCourt] set Nvalue={item.Nval2},EnteredDate=getDate() WHERE Id={foundId2}");

                    }
                    int foundId3 = await _sjcService.QueryRaw<int?>($"SELECT top 1 id FROM AppInputCourt WHERE AppId={item.AppId} and courtId={courtId ?? 0} and MetricsFieldId={nMetricsFieldId} and PlannedYear={currentYear + 2}") ?? 0;
                    if (foundId3 == 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"INSERT INTO [dbo].[AppInputCourt]([AppId],[CourtId],[MetricsFieldId],[PlannedYear],[Nvalue],[EnteredDate])
                                VALUES({appId ?? 0},{courtId ?? 0},{nMetricsFieldId},{currentYear + 2},{item.Nval3},getDate())");
                    }
                    else if (foundId3 > 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"Update [dbo].[AppInputCourt] set Nvalue={item.Nval3},EnteredDate=getDate() WHERE Id={foundId3}");

                    }
                    int foundId4 = await _sjcService.QueryRaw<int?>($"SELECT top 1 id FROM AppInputCourt WHERE AppId={item.AppId} and courtId={courtId ?? 0} and MetricsFieldId={nMetricsFieldId} and PlannedYear={currentYear + 3}") ?? 0;
                    if (foundId4 == 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"INSERT INTO [dbo].[AppInputCourt]([AppId],[CourtId],[MetricsFieldId],[PlannedYear],[Nvalue],[EnteredDate])
                                VALUES({appId ?? 0},{courtId ?? 0},{nMetricsFieldId},{currentYear + 3},{item.Nval4},getDate())");
                    }
                    else if (foundId4 > 0)
                    {
                        _ = await _sjcService.ExecuteRawSql($@"Update [dbo].[AppInputCourt] set Nvalue={item.Nval3},EnteredDate=getDate() WHERE Id={foundId4}");

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


        }
    }
}
