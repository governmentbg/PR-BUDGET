using AutoMapper;
using CielaDocs.Application.Models;
using CielaDocs.Application;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CielaDocs.Shared.Services;

namespace CielaDocs.SjcWeb.Areas.CourtUser.Controllers
{
    [Area("CourtUser")]
    [Authorize(Policy = "CourtUserOnly")]
    public class NomsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;

        public NomsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor,ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
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
        public async Task<JsonResult> GetFunctionalSubAreaByCourtId(int? courtId)
        {
           
                var returner = await _sjcRepo.GetProgramByCourtIdAsync(courtId ?? 0);
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
                return Json(data.Where(x => x.Id == id).FirstOrDefault());
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
        public async Task<JsonResult> CheckPeriod1Locked(int? programId, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<ProgramDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM ProgramDataLocked a
                    left join users u on a.LockedBy=u.id
                    where FunctionalSubAreaId={programId ?? 0} and Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<ProgramDataLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod2Locked(int? courtId, int? nm, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MainDataItemLockedVm>($@"SELECT TOP 1 a.Id,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM MainDataItemLocked a
                    left join users u on a.LockedBy=u.id
                    where a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0} ");
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataItemLockedVm>());
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckPeriod3Locked(int? programId, int? courtId, int? nm, int? ny)
        {
            try
            {
                var data = await _sjcService.QueryRaw<MainDataLockedVm>($@"SELECT TOP 1 a.Id,a.FunctionalSubAreaId,a.CourtId,a.Nmonth,a.Nyear,a.LockedBy,a.LockedOn, CONCAT( u.FirstName,' ', u.LastName) as LockedByUserName 
                    FROM MainDataLocked a
                    left join users u on a.LockedBy=u.id
                    where a.FunctionalSubAreaId={programId ?? 0} and a.CourtId={courtId ?? 0} and a.Nmonth={nm ?? 0} and a.Nyear={ny ?? 0} ");
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
        public async Task<bool> CheckUserCanLock(int userId, int lockedItemId)
        {
            try
            {
                var data = await _sjcService.QueryRaw<int?>($@"SELECT TOP 1 Id from UserLock where UserId={userId} and UserLockedItemId={lockedItemId}");
                return (data != null);
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
    }
}

