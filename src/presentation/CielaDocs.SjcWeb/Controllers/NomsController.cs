using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Shared.Repository;
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
using System.Globalization;
using CielaDocs.Application.Features.CourtTypes.Queries;
using Microsoft.Graph.TermStore;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using DevExpress.Export;

namespace CielaDocs.SjcWeb.Controllers
{
  
    public class NomsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;

        public NomsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, 
            ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor,
            ISjcService sjcService,ISjcServiceV2 sjcServiceV2)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
            _sjcService= sjcService;
            _sjcServiceV2= sjcServiceV2;
        }
        [HttpGet]
        public async Task<JsonResult> GetActivePeriodYear() {
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
        public async Task<JsonResult> GetInActivePeriodYear()
        {
            try
            {
                var data = await _sjcServiceV2.GetInActiveBudgetPeriodsAsync();
                var items = new List<IdNames>();
                foreach (var item in data)
                {
                    items.Add(new IdNames() { Id = item.Y1 ?? 0, Name = item?.Y1.ToString() });
                }

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
        public async Task<JsonResult> GetInActiveBudgetPeriod()
        {
            try
            {
                var data = await _sjcServiceV2.GetInActiveBudgetPeriodsAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<BudgetPeriodVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowMainDataAnalize(int? typeOfResultId,int? functionalSubAreaId,int? courtTypeId,int? nm, int? ny) {
            try
            {
                if (typeOfResultId == 1)
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                    FROM Court c
                                    LEFT JOIN MainData  t ON c.Id = t.CourtId 
                                        AND t.FunctionalSubAreaId = {functionalSubAreaId??0}
                                        AND t.NMonth = {nm??0}
                                        AND t.NYear = {ny??0}
	                                    left join CourtType a on c.CourtTypeId=a.Id
                                    WHERE a.id={courtTypeId??0} and t.CourtId IS NULL;");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                        FROM Court c
                                        JOIN MainData t ON c.Id = t.CourtId
                                        left join CourtType a on c.CourtTypeId=a.Id
                                        WHERE a.id={courtTypeId ?? 0} and t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        AND t.NMonth = {nm ?? 0}
                                        AND t.NYear = {ny ?? 0}");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowProgramDataAnalize(int? typeOfResultId, int? ny, int? functionalSubAreaId)
        {
            try
            {
                if (typeOfResultId == 1)
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                    FROM Court c
                                    LEFT JOIN ProgramDataCourt  t ON c.Id = t.CourtId 
                                        AND t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        AND t.PlannedYear = {ny ?? 0}
	                                    left join CourtInProgram a on c.Id=a.CourtId and a.FunctionalSubAreaId={functionalSubAreaId ?? 0}
                                    WHERE  (t.CourtId IS NULL or t.Nvalue is null )  and a.FunctionalSubAreaId={functionalSubAreaId ?? 0}");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                        FROM Court c
                                        JOIN ProgramDataCourt t ON c.Id = t.CourtId
                                        left join CourtInProgram a on c.Id=a.CourtId and a.FunctionalSubAreaId={functionalSubAreaId ?? 0}
                                        WHERE  t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        AND t.PlannedYear = {ny ?? 0} and t.Nvalue>0");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> ShowProgramDataAllAnalize(int? typeOfResultId, int? ny)
        {
            try
            {
                if (typeOfResultId == 1)
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                    FROM Court c
                                    LEFT JOIN ProgramDataCourt  t ON c.Id = t.CourtId 
                                        AND t.PlannedYear = {ny ?? 0}
	                                    left join CourtInProgram a on c.Id=a.CourtId 
                                    WHERE  (t.CourtId IS NULL or t.Nvalue is null ) ");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                        FROM Court c
                                        JOIN ProgramDataCourt t ON c.Id = t.CourtId
                                        left join CourtInProgram a on c.Id=a.CourtId 
                                        where t.PlannedYear = {ny ?? 0} and t.Nvalue>0");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
       
        public async Task<JsonResult> ShowKontoMonthDataAllAnalize(int? typeOfResultId, int? ny, int? nm)
        {
            if((ny is null)||(ny<2000)) return Json(new List<IdNames>());
            if ((nm is null) || (nm < 1)) return Json(new List<IdNames>());
            try
            {
                if (typeOfResultId == 1)
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                    FROM Court c
                                    LEFT JOIN KontoMonthData  t ON c.Id = t.CourtId 
                                        AND t.NYear = {ny ?? 0}
                                        AND t.NMonth = {nm ?? 0}
                                    WHERE   t.CourtId IS NULL;");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                        FROM Court c
                                        JOIN KontoMonthData t ON c.Id = t.CourtId
                                        WHERE t.NYear = {ny ?? 0}
                                        AND  t.NMonth = {nm ?? 0} and t.CourtId is not null");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]

        public async Task<JsonResult> ShowDraftBudgetAnalize(int? typeOfResultId, int? importTypeId,int? courtTypeId,int? institutionTypeId,int? ny,int? functionalSubAreaId)
        {
            if ((ny is null) || (ny < 2000)) return Json(new List<IdNames>());
            if (importTypeId is null)  return Json(new List<IdNames>());
            try
            {
                if (typeOfResultId == 1)
                {
                    if (importTypeId == 1)
                    {
                        var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                    FROM InstitutionType c
                                    LEFT JOIN ProgramDataInstitution  t ON c.Id = t.InstitutionTypeId
                                         AND t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        AND t.PlannedYear = {ny ?? 0}
                                    WHERE c.id={institutionTypeId??0} and (t.InstitutionTypeId IS NULL or t.Nvalue is null )");
                        return Json(data);
                    }
                    else {
                        var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                    FROM Court c
                                    LEFT JOIN ProgramDataCourt  t ON c.Id = t.CourtId 
                                        AND t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        AND t.PlannedYear = {ny ?? 0}
	                                    left join CourtType a on c.CourtTypeId=a.Id
                                    WHERE a.id={courtTypeId ?? 0} and (t.CourtId IS NULL or t.Nvalue is null )");
                        return Json(data);
                    }
                }
                else
                {
                    if (importTypeId == 1)
                    {
                        var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                        FROM InstitutionType c
                                        JOIN ProgramDataInstitution t ON c.Id = t.InstitutionTypeId
                                        AND t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        WHERE c.Id={institutionTypeId??0} and t.InstitutionTypeId = {institutionTypeId ?? 0}
                                        AND t.PlannedYear = {ny ?? 0} and t.nvalue is not null ");
                        return Json(data);
                    }
                    else {
                        var data = await _sjcService.QueryRawList<IdNames>($@"SELECT DISTINCT c.Id, c.Name 
                                        FROM Court c
                                        JOIN ProgramDataCourt t ON c.Id = t.CourtId
                                        left join CourtType a on c.CourtTypeId=a.Id
                                        WHERE a.id={courtTypeId ?? 0} and t.FunctionalSubAreaId = {functionalSubAreaId ?? 0}
                                        AND t.PlannedYear = {ny ?? 0} and t.nvalue is not null ");
                        return Json(data);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        
        [HttpGet]
        public async Task<JsonResult> GetInstitutionTypes()
        {
            try
            {
                var data = await _sjcRepo.GetInstitutionsAsync();
                return Json(data.ToList());
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
        public async Task<JsonResult> GetCourtTypesByProgramId(int? programId, string term="")
        {

            var returner = await _sjcService.QueryRawList<IdNames>($@"select distinct(a.Id),a.Name
                  from courtType a
                  left join Court c on a.id=c.CourtTypeId
                  left join CourtInProgram p on c.id=p.CourtId
                  where p.FunctionalSubAreaId={programId ?? 0} and a.Name like '{term}%'");
            return Json(returner.ToArray());

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCourtTypesbyInstitutionTypeId(int? institutionTypeId,string term = "" )
        {

            var returner = await _mediator.Send(new GetCourtTypesByInstitutionTypeQuery { Name = term, InstitutionTypeId= institutionTypeId??0 });
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
        public async Task<JsonResult> GetFunctionalSubAreaByCourtId(int? importType,int? courtId,int? institutionTypeId)
        {
            if (importType == 0)
            {
                var returner = await _sjcRepo.GetProgramByCourtIdAsync(courtId ?? 0);
                return Json(returner.ToArray());
            }
            else {
                var returner = await _sjcRepo.GetProgramByInstitutionTypeIdAsync(institutionTypeId??0);
                return Json(returner.ToArray());
            }

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetAllFunctionalSubAreas()
        {
                var returner = await _sjcRepo.GetAllProgramsAsync();
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
        public async Task<JsonResult> GetFunctionalSubArea(string term = "")
        {

            var returner = await _mediator.Send(new GetFnSubAreaQuery { Name = term });
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

            var returner = await _mediator.Send(new GetFunctionalSubAreaByAreaIdQuery { Id=id??0, Name = term });
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
        public async Task<JsonResult> GetCurrencies()
        {
            try
            {
                var data = await _sjcRepo.GetCurrencies();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCurrencYMeasure()
        {
            try
            {
                var data = await _sjcRepo.GetCurrencyMeasures();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCurrencyById(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetCurrencies();
                return Json(data.Where(x=>x.Id==id).FirstOrDefault());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetCurrencyMeasureById(int? id)
        {
            try
            {
                var data = await _sjcRepo.GetCurrencyMeasures();
                return Json(data.Where(x => x.Id == id).FirstOrDefault());
            }
            catch (Exception ex)
            {
                return Json(new List<IdNames>());
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
        public async Task<JsonResult> GetMetricsFields()
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
        public async Task<JsonResult> CheckPeriod1Locked(int? programId, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<ProgramDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM ProgramDataLocked a
                    left join users u on a.LockedBy=u.id
                    where FunctionalSubAreaId={programId??0} and Nyear={ny??0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod2Locked(int? courtId,int? nm, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MainDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM MainDataItemLocked a
                    left join users u on a.LockedBy=u.id
                    where a.CourtId={courtId ?? 0} and a.Nmonth={nm??0} and a.Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod3Locked(int? programId,int? courtId, int? nm, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM MainDataLocked a
                    left join users u on a.LockedBy=u.id
                    where a.FunctionalSubAreaId={programId??0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod4Locked(int? programId, int? courtId, int? nm, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MainDataPeriodLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM MainDataPeriodLocked a
                    left join users u on a.LockedBy=u.id
                    where a.FunctionalSubAreaId={programId ?? 0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataPeriodLockedVm>());
            }
        }
        public async Task<bool> CheckUserCanLock(int userId, int lockedItemId) {
            try
            {
                var data = await _sjcService.QueryRaw<int?>($@"SELECT TOP 1 Id from UserLock where UserId={userId} and UserLockedItemId={lockedItemId}");
                return (data!=null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod5Locked(int? programId, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<ApprovedDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM ApprovedDataItemLocked a
                    left join users u on a.LockedBy=u.id
                    where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<ApprovedDataItemLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowImportKontoLocked(int? ny)
        {
            try
            {
                if (ny == null)
                {
                    var data = await _sjcService.QueryRawList<KontoMonthDataLockedVm>($@"SELECT  a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM KontoMonthDataLocked a
                    left join users u on a.LockedBy=u.id");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRawList<KontoMonthDataLockedVm>($@"SELECT  a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM KontoMonthDataLocked a
                    left join users u on a.LockedBy=u.id
                    where  Nyear={ny ?? 0} ");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<ApprovedDataItemLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowImportPbKontoLocked(int? importType, int? courtId, int? institutionTypeId, int? ny)
        {
            try
            {
                if (importType == 1)
                {
                    var data = await _sjcService.QueryRawList<KontoPbInstitutionTypeLockedVm>($@"SELECT  a.Id,a.InstitutionTypeId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,t.Name as Name 
                    FROM KontoPbInstitutionTypeLocked a
                    left join users u on a.LockedBy=u.id
                    left join InstitutionType t on a.InstitutionTypeId=t.id
                    where a.InstitutionTypeId={institutionTypeId ?? 0} and a.Nyear={ny ?? 0} ");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRawList<KontoPbCourtLockedVm>($@"SELECT a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as Name 
                    FROM KontoPbCourtLocked a
                    left join users u on a.LockedBy=u.id
                    left join court c on a.CourtId=c.id
                    where a.CourtId={courtId ?? 0} and a.Nyear={ny ?? 0} ");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<KontoPbCourtLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowMainPeriodItemLocked(int? ny)
        {
            try
            {
               
                    var data = await _sjcService.QueryRawList<MainPeriodItemLockedVm>($@"SELECT a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as CourtName 
                    FROM MainPeriodItemLocked a
                    left join users u on a.LockedBy=u.id
                    left join court c on a.CourtId=c.id
                    where a.Nyear={ny ?? 0} ");
                    return Json(data);
                
            }
            catch (Exception ex)
            {
                return Json(new List<MainPeriodItemLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowProgramDataLocked(int? ny)
        {
            try
            {

                var data = await _sjcService.QueryRawList<ProgramDataLockedVm>($@"SELECT a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,f.Name as ProgramName
                              FROM ProgramDataLocked a
                            left join users u on a.LockedBy=u.id
                            left join FunctionalSubArea f on a.FunctionalSubAreaId=f.id
                            where a.Nyear={ny ?? 0} ");
                return Json(data);

            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowMainDataItemLocked(int? ny)
        {
            try
            {

                var data = await _sjcService.QueryRawList<MainDataItemLockedVm>($@"SELECT a.Id,a.CourtId,a.NMonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as CourtName
                              FROM MainDataItemLocked a
                            left join users u on a.LockedBy=u.id
                            left join Court c on a.CourtId=c.id
                            where a.Nyear={ny ?? 0} ");
                return Json(data);

            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowMainDataLocked(int? ny)
        {
            try
            {

                var data = await _sjcService.QueryRawList<MainDataLockedVm>($@"SELECT a.Id,a.FunctionalSubAreaId,a.CourtId,a.NMonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as CourtName,f.Name as ProgramName
                              FROM MainDataLocked a
                            left join users u on a.LockedBy=u.id
                            left join Court c on a.CourtId=c.id
                            left join FunctionalSubArea f on a.FunctionalSubAreaId=f.id
                            where a.Nyear={ny ?? 0} ");
                return Json(data);

            }
            catch (Exception ex)
            {
                return Json(new List<MainDataLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowMainDataPeriodLocked(int? ny)
        {
            try
            {

                var data = await _sjcService.QueryRawList<MainDataPeriodLockedVm>($@"SELECT a.Id,a.FunctionalSubAreaId,a.CourtId,a.NMonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as CourtName,f.Name as ProgramName
                              FROM MainDataPeriodLocked a
                            left join users u on a.LockedBy=u.id
                            left join Court c on a.CourtId=c.id
                            left join FunctionalSubArea f on a.FunctionalSubAreaId=f.id
                            where a.Nyear={ny ?? 0} ");
                return Json(data);

            }
            catch (Exception ex)
            {
                return Json(new List<MainDataPeriodLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> ShowApprovedDataItemLocked(int? ny)
        {
            try
            {

                var data = await _sjcService.QueryRawList<ApprovedDataItemLockedVm>($@"SELECT a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,f.Name as ProgramName
                            FROM ApprovedDataItemLocked a
                            left join users u on a.LockedBy=u.id
                            left join FunctionalSubArea f on a.FunctionalSubAreaId=f.id
                            where a.Nyear={ny ?? 0} ");
                return Json(data);

            }
            catch (Exception ex)
            {
                return Json(new List<ApprovedDataItemLockedVm>());
            }
        }
        


        [HttpGet]
        public async Task<JsonResult> CheckPeriod7Locked(int? importType, int? courtId, int? institutionTypeId, int? ny)
        {
            try
            {
                if (importType == 1)
                {
                    var data = await _sjcService.QueryRaw<KontoPbInstitutionTypeLockedVm>($@"SELECT TOP 1 a.Id,a.InstitutionTypeId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,t.Name as Name 
                    FROM KontoPbInstitutionTypeLocked a
                    left join users u on a.LockedBy=u.id
                    left join InstitutionType t on a.InstitutionTypeId=t.id
                    where a.InstitutionTypeId={institutionTypeId ?? 0} and a.Nyear={ny ?? 0} ");
                    return Json(data);
                }
                else
                {
                    var data = await _sjcService.QueryRaw<KontoPbCourtLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as Name 
                    FROM KontoPbCourtLocked a
                    left join users u on a.LockedBy=u.id
                    left join court c on a.CourtId=c.id
                    where a.CourtId={courtId ?? 0} and a.Nyear={ny ?? 0} ");
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                return Json(new List<KontoPbCourtLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod8Locked(int? courtId, int? nm, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MainPeriodItemLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName,c.Name as CourtName 
                    FROM MainPeriodItemLocked a
                    left join users u on a.LockedBy=u.id
                    left join Court c on a.CourtId=c.id
                    where a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<MainPeriodItemLockedVm>());
            }
        }
        [HttpPost]
        public async Task<JsonResult> LockProgramData(int? typeoflock, int? programId, int? ny) {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<ProgramDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn FROM ProgramDataLocked a where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0}");

            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 1);
            if(!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null)
                {
                    _ = await _sjcService.ExecuteRawSql($"Insert into ProgramDataLocked (FunctionalSubAreaId ,Nyear,LockedBy) VALUES ({programId ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                    return Json(new { success = true, msg = "Периодът бе заключен" });
                }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });
            }
            if (typeoflock == 1)
            {
                 if(rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if(rec?.LockedBy!=empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  ProgramDataLocked where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> LockMainDataItem(int? typeoflock, int? courtId, int? nm, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataItemLocked a where a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");

            //2 means userLockedItem=>id=2, name=2.Входни данни
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 2);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null)
                {
                    _ = await _sjcService.ExecuteRawSql($"Insert into MainDataItemLocked (CourtId,Nmonth ,Nyear,LockedBy) VALUES ({courtId ?? 0},{nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                    return Json(new { success = true, msg = "Периодът бе заключен" });
                }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });
            }
            if (typeoflock == 1)
            {
                if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  MainDataItemLocked where CourtId={courtId ?? 0} and Nmonth={nm??0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> LockMainData(int? typeoflock, int? programId, int? courtId, int? nm, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataLocked a where a.FunctionalSubAreaId={programId ?? 0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");

            //3 means userLockedItem=>id=3, name=3.Данни за показатели-месечни
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 3);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null)
                {
                    _ = await _sjcService.ExecuteRawSql($"Insert into MainDataLocked (FunctionalSubAreaId,CourtId,Nmonth ,Nyear,LockedBy) VALUES ({programId ?? 0},{courtId ?? 0},{nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                    return Json(new { success = true, msg = "Периодът бе заключен" });
                }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });
            }
            if (typeoflock == 1)
            {
                              if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  MainDataLocked where FunctionalSubAreaId={programId ?? 0} and CourtId={courtId ?? 0} and Nmonth={nm ?? 0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> LockMainDataPeriod(int? typeoflock, int? programId, int? courtId, int? nm, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataPeriodLocked a where a.FunctionalSubAreaId={programId ?? 0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");

            //4 means userLockedItem=>id=4, name=4.Данни за показатели за период
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 4);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null)
                {
                    _ = await _sjcService.ExecuteRawSql($"Insert into MainDataPeriodLocked (FunctionalSubAreaId,CourtId,Nmonth ,Nyear,LockedBy) VALUES ({programId ?? 0},{courtId ?? 0},{nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                    return Json(new { success = true, msg = "Периодът бе заключен" });
                }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });
            }
            if (typeoflock == 1)
            {
                               if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  MainDataPeriodLocked where FunctionalSubAreaId={programId ?? 0} and CourtId={courtId ?? 0} and Nmonth={nm ?? 0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> LockApprovedDataItem(int? typeoflock, int? programId, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<ApprovedDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn FROM ApprovedDataItemLocked a where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0}");
            //5 means userLockedItem=>id=5, name=5.Утвърден бюджет
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 5);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null) { 
                _ = await _sjcService.ExecuteRawSql($"Insert into ApprovedDataItemLocked (FunctionalSubAreaId ,Nyear,LockedBy) VALUES ({programId ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                return Json(new { success = true, msg = "Периодът бе заключен" });
                 }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });
            }
            if (typeoflock == 1)
            {
               
                if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  ApprovedDataItemLocked where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> LockImportKonto(int? typeoflock, int? nm, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<KontoMonthDataLockedVm>($@"SELECT TOP 1 a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM KontoMonthDataLocked a where a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");
            //6 means userLockedItem=>id=6, name=6.Импорт на данни от Конто
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 6);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null)
                {
                    _ = await _sjcService.ExecuteRawSql($"Insert into KontoMonthDataLocked (Nmonth ,Nyear,LockedBy) VALUES ({nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                    return Json(new { success = true, msg = "Периодът бе заключен" });
                }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });

            }
            if (typeoflock == 1)
            {
               
                if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  KontoMonthDataLocked where Nmonth={nm ?? 0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> LockImportPbKonto(int? typeoflock, int? importType, int? courtId, int? institutionTypeId, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }

            if (importType == 1)
            {
                var rec = await _sjcService.QueryRaw<KontoPbInstitutionTypeLockedVm>($@"SELECT TOP 1 a.Id,a.InstitutionTypeId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbInstitutionTypeLocked a where a.InstitutionTypeId={institutionTypeId ?? 0} and a.Nyear={ny ?? 0}");
                //7 means userLockedItem=>id=7, name=7.Импорт на данни проекто бюджет
                var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 6);
                if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
                if (typeoflock == 0)
                {
                    if (rec is null)
                    {
                        _ = await _sjcService.ExecuteRawSql($"Insert into KontoPbInstitutionTypeLocked (InstitutionTypeId ,Nyear,LockedBy) VALUES ({institutionTypeId ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                        return Json(new { success = true, msg = "Периодът бе заключен" });
                    }
                    else return Json(new { success = true, msg = "Периодът вече е заключен!" });

                }
                if (typeoflock == 1)
                {

                    if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                    if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                    _ = await _sjcService.ExecuteRawSql($"Delete from  KontoPbInstitutionTypeLocked where InstitutionTypeId={institutionTypeId?? 0} and Nyear={ny ?? 0} ");
                    return Json(new { success = true, msg = "Периодът бе отключен" });
                }
                return Json(new { success = true, msg = "Неуточнено действие" });
            }
            else
            {
                var rec = await _sjcService.QueryRaw<KontoPbCourtLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbCourtLocked a where a.CourtId={courtId ?? 0} and a.Nyear={ny ?? 0}");
                //7 means userLockedItem=>id=7, name=7.Импорт на данни проекто бюджет
                var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 6);
                if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
                if (typeoflock == 0)
                {
                    if (rec is null)
                    {
                        _ = await _sjcService.ExecuteRawSql($"Insert into KontoPbCourtLocked (CourtId ,Nyear,LockedBy) VALUES ({courtId ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                        return Json(new { success = true, msg = "Периодът бе заключен" });
                    }
                    else return Json(new { success = true, msg = "Периодът вече е заключен!" });

                }
                if (typeoflock == 1)
                {

                    if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                    if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                    _ = await _sjcService.ExecuteRawSql($"Delete from  KontoPbCourtLocked where CourtId={courtId ?? 0} and Nyear={ny ?? 0} ");
                    return Json(new { success = true, msg = "Периодът бе отключен" });
                }
                return Json(new { success = true, msg = "Неуточнено действие" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> DeleteImportKontoById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<KontoMonthDataLockedVm>($@"SELECT TOP 1 a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM KontoMonthDataLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  KontoMonthDataLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteImportPbKontoById(int? id,int importType)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            if (importType == 1)
            {
                var rec = await _sjcService.QueryRaw<KontoPbInstitutionTypeLockedVm>($@"SELECT TOP 1 a.Id,a.InstitutionTypeId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbInstitutionTypeLocked a where a.id={id ?? 0}");
                if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  KontoPbInstitutionTypeLocked where id={id ?? 0}");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            else
            {
                var rec = await _sjcService.QueryRaw<KontoPbCourtLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nyear,a.LockedBy,a.LockedOn FROM KontoPbCourtLocked a where a.id={id ?? 0}");
                if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  KontoPbCourtLocked where id={id ?? 0}");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> LockMainPeriodItem(int? typeoflock, int? courtId, int? nm, int? ny)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainPeriodItemLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainPeriodItemLocked a where a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");

            //8 means userLockedItem=>id=8, name=8.Импорт на входни данни
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 8);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                if (rec is null)
                {
                    _ = await _sjcService.ExecuteRawSql($"Insert into MainPeriodItemLocked (CourtId,Nmonth ,Nyear,LockedBy) VALUES ({courtId ?? 0},{nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                    return Json(new { success = true, msg = "Периодът бе заключен" });
                }
                else return Json(new { success = true, msg = "Периодът вече е заключен!" });
            }
            if (typeoflock == 1)
            {
                if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
                if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
                _ = await _sjcService.ExecuteRawSql($"Delete from  MainPeriodItemLocked where CourtId={courtId ?? 0} and Nmonth={nm ?? 0} and Nyear={ny ?? 0} ");
                return Json(new { success = true, msg = "Периодът бе отключен" });
            }
            return Json(new { success = true, msg = "Неуточнено действие" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMainPeriodItemLockedById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainPeriodItemLockedVm>($@"SELECT TOP 1 a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainPeriodItemLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  MainPeriodItemLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteProgramDataLockedById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<ProgramDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn FROM ProgramDataLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  ProgramDataLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMainDataItemLockedById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.Court,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataItemLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  MainDataItemLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMainDataLockedById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Court,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  MainDataLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMainDataPeriodLockedById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<MainDataPeriodLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Court,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataPeriodLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  MainDataPeriodLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteApprovedDataItemLockedById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<ApprovedDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn FROM ApprovedDataItemLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  ApprovedDataItemLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }


    }
}
