using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Shared.Repository;
using CielaDocs.AdminPanel.Extensions;
using CielaDocs.AdminPanel.Models;





using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Services;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class NomsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;

        public NomsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;
        }
        [HttpGet]
        public async Task<JsonResult> GetActivePeriodYear()
        {
            try
            {
                var data = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                var items = new List<IdNames>() {
                    new IdNames() { Id=data.Y1??0, Name=data?.Y1.ToString() },
                    new IdNames() { Id=data.Y2??0, Name=data?.Y2.ToString() },
                    new IdNames() { Id=data.Y3??0, Name=data?.Y3.ToString() },
                    new IdNames() { Id=data.Y4??0, Name=data?.Y4.ToString() },
                };

                return Json(items);
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetActiveBudgetPeriod()
        {
            try
            {
                var data = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<BudgetPeriodVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetActiveBudgetPeriodById(int id)
        {
            try
            {
                var data = await _sjcServiceV2.GetActiveBudgetPeriodByIdAsync(id);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<BudgetPeriodVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetInstitutionTypes()
        {
            try
            {
                var data = await _sjcRepo.GetInstitutionsAsync();
                return Json(data.Where(x => x.Id > 0).ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAllCourts()
        {
            try
            {
                var data = await _sjcRepo.GetCourtsAsync();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCourtTypeByInstitutionTypeId(int? institutionTypeId)
        {
            try
            {
                var data = await _sjcRepo.GetCourtTypeByInstitutionTypeIdAsync(institutionTypeId ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetInstitutionTypeById(int? typeId)
        {
            try
            {
                var data = await _sjcRepo.GetInstitutionTypeByIdAsync(typeId ?? 0);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCourtsByCourtTypeId(int? courtTypeId)
        {
            try
            {
                var data = await _sjcRepo.GetCourtsByCourtTypeIdAsync(courtTypeId ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]

        public async Task<JsonResult> GetCourts()
        {
            var data = await _mediator.Send(new GetCourtComboQuery { Name = string.Empty });
            return Json(data.ToList());
        }
        [HttpGet]

        public async Task<JsonResult> GetCourtsSelect2(string term = "")
        {
            var data = await _mediator.Send(new GetCourtComboQuery { Name = term });
            return Json(data.ToList());
        }
        [HttpGet]

        public async Task<JsonResult> GetCourtsByCourtTypeIdSelect2(int? courtTypeId, string term = "")
        {
            var data = await _mediator.Send(new GetCourtByCourtTypeIdComboQuery { CourtTypeId = courtTypeId ?? 0, Name = term });
            return Json(data.ToList());
        }
        [HttpGet]

        public async Task<JsonResult> GetCourtById(int id)
        {
            var data = await _mediator.Send(new GetCourtByIdQuery { Id = id });
            return Json(data);
        }
        [HttpGet]
        public async Task<string> GetCourtDetails(int? courtId)
        {
            string ret = string.Empty;
            var onrs = await _mediator.Send(new GetCourtByIdQuery { Id = courtId ?? 0 });
            if (onrs != null)
            {
                // ret += $"{onrs?.Nasme?.NasmeName},област {onrs?.Nasme?.MunicipalityDtos?.RegionDtos?.Name}, община {onrs?.Nasme?.MunicipalityDtos?.Name}, {onrs?.Address}, имейл:{onrs?.Email}, тел.{onrs?.Phone}";
            }
            return ret;
        }

        public async Task<IActionResult> CourtDetails(int? courtId)
        {
            ViewBag.OnrId = courtId ?? 0;
            var court = await _mediator.Send(new GetCourtByIdQuery { Id = courtId ?? 0 });
            _ = int.TryParse(User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value?.ToString(), out int UserId);
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Преглед на {court?.Name} от {User?.Identity?.Name ?? string.Empty}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = UserId, MsgId = 0, Msg = logmsg, IP = ip });
            return View(court);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetTowns(string term = "")
        {

            var returner = await _mediator.Send(new GetTownQuery { Name = term });
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetTownById(int? id)
        {

            var returner = await _mediator.Send(new GetTownByIdQuery { TownId = id ?? 0 });

            return Json(new { Id = returner?.Id ?? 0, Name = returner?.Name ?? String.Empty, success = true });
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCourtTypes(string term = "")
        {

            var returner = await _mediator.Send(new GetCourtTypesQuery { Name = term });
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCourtTypeById(int? id)
        {

            var returner = await _mediator.Send(new GetCourtTypeByIdQuery { Id = id ?? 0 });

            return Json(new { Id = returner?.Id ?? 0, Name = returner?.Name ?? String.Empty, success = true });
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetUserTypes(string term = "")
        {

            var returner = await _mediator.Send(new GetUserTypesComboQuery { Name = term });
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetUserTypeById(int? id)
        {

            var returner = await _mediator.Send(new GetUserTypeByIdComboQuery { Id = id ?? 0 });

            return Json(new { Id = returner?.Id ?? 0, Name = returner?.Name ?? String.Empty, success = true });
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetFunctionalAreas(string term = "")
        {

            var returner = await _mediator.Send(new GetFunctionalAreasQuery { Name = term });
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetFunctionalAreaById(int? id)
        {

            var returner = await _mediator.Send(new GetFunctionalAreabyIdQuery { Id = id ?? 0 });
            return Json(returner);

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetInstitutions(string term = "")
        {

            var returner = await _sjcRepo.GetInstitutionsAsync();
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetFunctionalSubArea(string term = "")
        {

            var returner = await _mediator.Send(new GetFnSubAreaQuery { Name = term });
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetFunctionalSubAreaByCourtId(int? courtId)
        {

            var returner = await _sjcRepo.GetProgramByCourtIdAsync(courtId ?? 0);
            return Json(returner.ToArray());

        }
        [HttpGet]

        public async Task<JsonResult> GetFunctionalSubAreaById(int? id)
        {
            var data = await _mediator.Send(new GetFnSubAreaByIdQuery { Id = id ?? 0 });
            return Json(data);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetFunctionalSubAreaByAreaId(int? id, string term = "")
        {

            var returner = await _mediator.Send(new GetFnSubAreaQuery { Name = term });
            return Json(returner.ToArray());

        }
        [HttpGet]
        public async Task<JsonResult> GetMainIndicatorsByProgramId(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetMainIndicatorsByProgramId(id ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAppById(int? id)
        {
            try
            {
                var data = await _sjcService.QueryRaw<IdNames>($"Select Id,Name from App where ID={id ?? 0}");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetApps()
        {
            try
            {
                var data = await _sjcService.QueryRawList<IdNames>($"Select Id,Name from App ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAppDefByProgramId(int? id)
        {
            try
            {
                var data = await _sjcService.QueryRawList<AppDefVm>($@"SELECT a.Id ,a.FunctionalSubAreaId ,a.AppId ,a.RowNum ,a.RowCode,a.Name,a.ParentRowNum,a.IsActive ,a.MeasureId,a.Formula,b.Name as AppName,c.Name as MeasureName
                     FROM  dbo.AppDef a
                    left join App b on a.appId=b.id
                    left join Measure c on a.MeasureID=c.id
                    where a.FunctionalSubAreaId={id ?? 0}");
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetProgramDefItemsByProgramId(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetProgramDefByProgramIdAsync(id ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsByProgramId(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetMetricsByProgramId(id ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsFields(string term = "")
        {
            try
            {
                var data = await _sjcRepo.GetMetricsFields();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MetricsField>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsFieldByCode(string term = "")
        {
            try
            {
                var data = await _sjcRepo.GetMetricsFields();
                data = data.Where(x => x.Code.Contains(term.ToUpper()));
                List<IdNames> ret = new List<IdNames>();
                if (data.Any())
                {
                    foreach (var item in data)
                    {
                        ret.Add(new IdNames() { Id = item?.Id ?? 0, Name = item?.Code + "-" + item?.Name });
                    }
                }
                return Json(ret);
            }
            catch (Exception ex)
            {
                return Json(new List<MetricsField>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAppDefMetricsFieldByAppDefId(int? id)
        {
            try
            {
                var data = await _sjcService.QueryRawList<AppDefMetricsFieldVm>($@"select a.Id,a.AppDefId,a.MetricsFieldId,m.Name as MetricsFieldName,m.Code as MetricsFieldCode,m.IsActive
                 from AppDefMetricsField a
                Left join MetricsField m on a.MetricsFieldId=m.id
                where a.AppDefId={id ?? 0}");
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MetricsField>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsFieldInProgramByMainIndicatorId(int id)
        {
            var data = await _sjcServiceV2.GetMetricsFieldInProgramByMainIndicatorIdAsync(id);
            return Json(data.ToList());
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMetricsFieldInProgramById(int id)
        {
            _ = await _sjcService.ExecuteRawSql($"Delete from MetricsFieldInProgram where id={id}");
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteAppDefMetricsFieldById(int id)
        {
            _ = await _sjcService.ExecuteRawSql($"Delete from AppDefMetricsField where id={id}");
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<MetricsField> GetMetricsFieldById(int id)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MetricsField>($"SELECT [Id],[Code],[Name],[NeededFor],[IsActive],[TypeOfIndicatorId] FROM MetricsField where id={id}"); ;
                return data;
            }
            catch (Exception ex)
            {
                return new MetricsField();
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddMetricsFieldInProgramById(int? mainIndicatorsId, int? functionalSubAreaId, int? metricsFieldId)
        {
            var mf = await GetMetricsFieldById(metricsFieldId ?? 0);
            _ = await _sjcService.ExecuteRawSql($@"INSERT INTO [MetricsFieldInProgram]([MainIndicatorsId],[FunctionalSubAreaId],[Code],[Name],[NeededFor],[IsActive],[TypeOfIndicatorId])
                VALUES({mainIndicatorsId ?? 0}
                ,{functionalSubAreaId ?? 0}
                ,'{mf?.Code}'
                ,'{mf?.Name}'
                ,'{mf?.NeededFor}'
                ,{1}
                ,{mf?.TypeOfIndicatorId ?? 0})");
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> AppDefMetricsField(int? appDefId, int? metricsFieldId)
        {

            _ = await _sjcService.ExecuteRawSql($@"INSERT INTO AppDefMetricsField([AppDefId],[MetricsFieldId])
                VALUES({appDefId ?? 0}
                ,{metricsFieldId ?? 0} )");
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<JsonResult> GetMeasure()
        {
            try
            {
                var data = await _sjcRepo.GetMeasureAsync();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsByCode()
        {
            try
            {
                var data = await _sjcRepo.GetMeasureAsync();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetTypeOfIndicator()
        {
            try
            {
                var data = await _sjcRepo.GetTypeOfIndicatorAsync();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetTypeOfIndicatorById(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetTypeOfIndicatorByIdAsync(id ?? 0);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMeasureById(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetMeasureByIdAsync(id ?? 0);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAllUserLockedItems()
        {
            try
            {
                var data = await _sjcService.GetAllUserLockedItems();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<UserLockedItemVm>());
            }
        }
        [HttpPost]

        public async Task<JsonResult> SynchronizeProgramData(int? id)
        {
            try
            {
                var cfg = await _sjcService.QueryRaw<Cfg>($"Select * from cfg");
                if (cfg?.CurrentYear > 0)
                {
                    _ = await _sjcRepo.Sp_UpdateProgramDataAsync(id, cfg?.CurrentYear ?? 0);
                    _ = await _sjcRepo.Sp_UpdateProgramDataCourtAsync(id, cfg?.CurrentYear ?? 0);
                    _ = await _sjcRepo.Sp_UpdateProgramDataInstitutionAsync(id, cfg?.CurrentYear ?? 0);
                    _ = await _sjcRepo.Sp_UpdateProgramDataProsecutorAsync(id, cfg?.CurrentYear ?? 0);
                }
                var abp = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                List<int?> years = new List<int?> { abp.Y1, abp.Y2, abp.Y3, abp.Y4 };
                for (int i = 0; i < years.Count; i++)
                {
                    _ = await _sjcRepo.Sp_UpdateProgramDataAsync(id, years[i]);
                    _ = await _sjcRepo.Sp_UpdateProgramDataCourtAsync(id, years[i]);
                    _ = await _sjcRepo.Sp_UpdateProgramDataInstitutionAsync(id, years[i]);
                    _ = await _sjcRepo.Sp_UpdateProgramDataProsecutorAsync(id, years[i]);
                }
                var data = await _sjcRepo.GetProgramDefByProgramIdAsync(id ?? 0);
                if (data.Any())
                {
                    _ = await _sjcService.ExecuteRawSql($"Update ProgramData set ValueAllowed=0 where functionalSubAreaId={id ?? 0}");
                    _ = await _sjcService.ExecuteRawSql($"Update ProgramDataCourt set ValueAllowed=0 where functionalSubAreaId={id ?? 0}");
                    _ = await _sjcService.ExecuteRawSql($"Update ProgramDataInstitution set ValueAllowed=0 where functionalSubAreaId={id ?? 0}");
                    _ = await _sjcService.ExecuteRawSql($"Update ProgramDataProsecutor set ValueAllowed=0 where functionalSubAreaId={id ?? 0}");
                    foreach (var itemVm in data)
                    {
                        if (itemVm.ValueAllowed == true)
                        {
                            _ = await _sjcService.ExecuteRawSql($"Update ProgramData set ValueAllowed=1 where functionalSubAreaId={id ?? 0} and RowNum={itemVm?.RowNum ?? 0}");
                            _ = await _sjcService.ExecuteRawSql($"Update ProgramDataCourt set ValueAllowed=1 where functionalSubAreaId={id ?? 0} and RowNum={itemVm?.RowNum ?? 0}");
                            _ = await _sjcService.ExecuteRawSql($"Update ProgramDataInstitution set ValueAllowed=1 where functionalSubAreaId={id ?? 0} and RowNum={itemVm?.RowNum ?? 0}");
                            _ = await _sjcService.ExecuteRawSql($"Update ProgramDataProsecutor set ValueAllowed=1 where functionalSubAreaId={id ?? 0} and RowNum={itemVm?.RowNum ?? 0}");
                        }
                    }
                    return Json(new { msg = "Данните бяха актуализирани", success = false });
                }
                else return Json(new { msg = "Липсват данни за актуализация", success = false });
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex?.Message, success = false });
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsInputByInstitutionTypeId(int? institutionTypeId)
        {
            try
            {
                var data = await _sjcService.QueryRawList<MetricsInputCodeName>($@"select i.Id,m.Code,m.Name from MetricsInput i
                  left join MetricsField m on i.MetricsFieldId=m.id
                  left join InstitutionType t on i.InstitutionTypeId=t.id
                  where t.id={institutionTypeId}");
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MetricsInputCodeName>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetMetricsInputByCourtId(int? courtId)
        {
            try
            {
                var data = await _sjcService.QueryRawList<MetricsInputCodeName>($@"select i.Id,m.Code,m.Name from MetricsInput i
                  left join MetricsField m on i.MetricsFieldId=m.id
                  left join InstitutionType t on i.InstitutionTypeId=t.id
                  left join CourtType ct on t.id=ct.InstitutionTypeId
                  left join Court c on ct.id=c.CourtTypeId
                  where c.id={courtId}");
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MetricsInputCodeName>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAppRequiredByAppId(int? appId)
        {
            try
            {
                var data = await _sjcService.QueryRawList<AppRequiredVm>($@"SELECT a.Id
                                  ,a.AppId
                                  ,a.InstitutionTypeId
                                  ,a.IsActive
	                            ,i.Name as InstitutionTypeName
                              FROM AppRequired a
                            left join InstitutionType i on a.InstitutionTypeId=i.id
                                                            where a.AppId={appId ?? 0}");
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
    }
}
