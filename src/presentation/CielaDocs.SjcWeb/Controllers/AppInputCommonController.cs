using CielaDocs.Application.Models;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CielaDocs.SjcWeb.Controllers
{
    public class AppInputCommonController : Controller
    {
        private readonly ILogger<AppInputCommonController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;
        private const int VssCreator = 4;
        public AppInputCommonController(ILogger<AppInputCommonController> logger, IWebHostEnvironment env, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
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
                    _ = await _sjcServiceV2.Sp_InitAppInputCommonAsync(VssCreator, years[i]);
                    await Task.Delay(500);
                }
            }
            ViewBag.AppId = filterData?.AppId;
            ViewBag.Year = filterData?.Nyear;
            ViewBag.AppName = app?.Name ?? string.Empty;
            ViewBag.IsLocked = filterData?.IsLocked ?? false;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetAppInputDataGrid()
        {

            var inp = HttpContext.Session.Get<FilterAppInputCommonVm>("FilterAppInputCommonSess") ?? new FilterAppInputCommonVm();
            string sql = $@"select a.Id,a.CreatedByInstTypeId,a.MetricsFieldId,a.PlannedYear,a.EnteredDate, m.Code as MetricsFieldCode,m.Name as MetricsFieldName,a.Nvalue as nval1,b.nvalue as nval2,c.Nvalue as nval3,d.Nvalue as nval4
                                   from AppInputCommon a
                                    left join AppInputCommon b on a.CreatedByInstTypeId=b.CreatedByInstTypeId and a.MetricsFieldId=b.MetricsFieldId and b.PlannedYear=a.PlannedYear+1
                                    left join AppInputCommon c on a.CreatedByInstTypeId=c.CreatedByInstTypeId and a.MetricsFieldId=c.MetricsFieldId and c.PlannedYear=a.PlannedYear+2
                                    left join AppInputCommon d on a.CreatedByInstTypeId=d.CreatedByInstTypeId and a.MetricsFieldId=d.MetricsFieldId and d.PlannedYear=a.PlannedYear+3
                                    left join MetricsField m on a.MetricsFieldId=m.id
                                 AND m.Id IN (
                                     SELECT DISTINCT admf.MetricsFieldId
                                     FROM AppDefMetricsField admf
                                     WHERE admf.AppDefId IN (
                                         SELECT ad.Id FROM AppDef ad WHERE ad.AppId ={inp?.AppId??0}
                                     )
                                 )
                                    where a.id>0 and a.CreatedByInstTypeId={inp?.CreatedByInstTypeId??0} and a.plannedYear={inp?.Nyear ?? 0} and m.code is not null
                                GROUP BY 
                                    a.Id,
                                    a.CreatedByInstTypeId,
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
                                    a.CreatedByInstTypeId,
                                    m.Code";
            try
            {
                var data = await _sjcService.QueryRawList<AppInputCommon3YVm>(sql);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<AppInputCommon3YVm>());
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

                _ = await UpdateAppInputData3YValueByIdAsync(key, name, n);



            }
            return Json(string.Empty);
        }
        private async Task<int> UpdateAppInputData3YValueByIdAsync(int? id, string fieldName, decimal? val)
        {
            var activePeriod = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
            int currentYear = activePeriod?.Y1 ?? 0;
            var rec= await _sjcService.QueryRaw<AppInputCommonVm>("SELECT * FROM AppInputCommon WHERE Id=@Id", new { Id = id });
            switch (fieldName.ToLower())
            {
                case "nval2": currentYear = currentYear + 1; break;
                case "nval3": currentYear = currentYear + 2; break;
                case "nval4": currentYear = currentYear + 3; break;
            }
            var sql = $@"UPDATE AppInputCommon SET Nvalue = {val}, EnteredDate=getDate() WHERE CreatedByInstTypeId={rec?.CreatedByInstTypeId??0} and MetricsFieldId={rec?.MetricsFieldId??0} and PlannedYear={currentYear}";
            _ = await _sjcService.ExecuteRawSql(sql);
            return 1;
        }
    }
}
