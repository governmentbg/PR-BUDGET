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

        public NomsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
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
                var data = await _sjcRepo.GetCourtsByCourtTypeIdAsync(courtTypeId??0);
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

        public async Task<JsonResult> GetCourtsByCourtTypeIdSelect2( int? courtTypeId, string term = "")
        {
            var data = await _mediator.Send(new GetCourtByCourtTypeIdComboQuery { CourtTypeId=courtTypeId??0, Name = term });
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
            string logmsg = $"Преглед на {court?.Name} от {User?.Identity?.Name??string.Empty}";
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

            var returner = await _sjcRepo.GetProgramByCourtIdAsync(courtId??0);
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
        public async Task<JsonResult> GetFunctionalSubAreaByAreaId(int? id,string term = "")
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
                var data = await _sjcRepo.GetTypeOfIndicatorByIdAsync(id??0);
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

    }
}
