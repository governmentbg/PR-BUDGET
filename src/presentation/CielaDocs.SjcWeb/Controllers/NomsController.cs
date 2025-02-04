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

        public NomsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor,ISjcService sjcService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
            _sjcService= sjcService;
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
        [HttpPost]
        public async Task<JsonResult> LockProgramData(int? typeoflock, int? programId, int? ny) {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 1);
            if(!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                _ = await _sjcService.ExecuteRawSql($"Insert into ProgramDataLocked (FunctionalSubAreaId ,Nyear,LockedBy) VALUES ({programId??0},{ny??0} ,{empl?.Id??0}) ");
                return Json(new { success = true, msg = "Периодът бе заключен" });
            }
            if (typeoflock == 1)
            {
                var rec=await _sjcService.QueryRaw<ProgramDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn FROM ProgramDataLocked a where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0}");
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
            //2 means userLockedItem=>id=2, name=2.Входни данни
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 2);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                _ = await _sjcService.ExecuteRawSql($"Insert into MainDataItemLocked (CourtId,Nmonth ,Nyear,LockedBy) VALUES ({courtId ?? 0},{nm??0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                return Json(new { success = true, msg = "Периодът бе заключен" });
            }
            if (typeoflock == 1)
            {
                var rec = await _sjcService.QueryRaw<MainDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataItemLocked a where a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");
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
            //3 means userLockedItem=>id=3, name=3.Данни за показатели-месечни
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 3);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                _ = await _sjcService.ExecuteRawSql($"Insert into MainDataLocked (FunctionalSubAreaId,CourtId,Nmonth ,Nyear,LockedBy) VALUES ({programId??0},{courtId ?? 0},{nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                return Json(new { success = true, msg = "Периодът бе заключен" });
            }
            if (typeoflock == 1)
            {
                var rec = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataLocked a where a.FunctionalSubAreaId={programId??0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");
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
            //4 means userLockedItem=>id=4, name=4.Данни за показатели за период
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 4);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                _ = await _sjcService.ExecuteRawSql($"Insert into MainDataPeriodLocked (FunctionalSubAreaId,CourtId,Nmonth ,Nyear,LockedBy) VALUES ({programId ?? 0},{courtId ?? 0},{nm ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                return Json(new { success = true, msg = "Периодът бе заключен" });
            }
            if (typeoflock == 1)
            {
                var rec = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM MainDataPeriodLocked a where a.FunctionalSubAreaId={programId ?? 0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");
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
            //5 means userLockedItem=>id=5, name=5.Утвърден бюджет
            var canUserLock = await CheckUserCanLock(empl?.Id ?? 0, 5);
            if (!canUserLock) { return Json(new { success = false, msg = "Нямате права за заключване или отключване на този период" }); }
            if (typeoflock == 0)
            {
                _ = await _sjcService.ExecuteRawSql($"Insert into ApprovedDataItemLocked (FunctionalSubAreaId ,Nyear,LockedBy) VALUES ({programId ?? 0},{ny ?? 0} ,{empl?.Id ?? 0}) ");
                return Json(new { success = true, msg = "Периодът бе заключен" });
            }
            if (typeoflock == 1)
            {
                var rec = await _sjcService.QueryRaw<ApprovedDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn FROM ApprovedDataItemLocked a where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0}");
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
            var rec = await _sjcService.QueryRaw<ApprovedDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM KontoMonthDataLocked a where a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0}");
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
        public async Task<JsonResult> DeleteImportKontoById(int? id)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl == null) { return Json(new { success = false, msg = "Невалиден указател към текущ потребител на системата" }); }
            var rec = await _sjcService.QueryRaw<ApprovedDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn FROM KontoMonthDataLocked a where a.id={id ?? 0}");
            if (rec is null) { return Json(new { success = false, msg = "Този период не е заключен за да го отключвате" }); }
            if (rec?.LockedBy != empl?.Id) { return Json(new { success = false, msg = "Този период не е заключен от вас. В този случай отключването се прекратява" }); }
            _ = await _sjcService.ExecuteRawSql($"Delete from  KontoMonthDataLocked where id={id ?? 0}");
            return Json(new { success = true, msg = "Периодът бе отключен" });
        }
    }
}
