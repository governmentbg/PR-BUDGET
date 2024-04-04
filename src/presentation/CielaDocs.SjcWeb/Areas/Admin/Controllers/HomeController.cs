using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Shared.Repository;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;

using DocumentFormat.OpenXml.ExtendedProperties;


using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CielaDocs.SjcWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminOnly")]
    public class HomeController : CommonController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IMediator mediator, IMapper mapper, ILogRepository logRepo, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });


            ViewBag.CourtId = empl?.CourtId ?? 0;
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Достъп до предприятия потребители от {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = empl?.Id??0, MsgId = 0, Msg = logmsg, IP = ip });
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetOwnerById(int? courtId)
        {
            try
            {
                var data = await _mediator.Send(new GetCourtByIdQuery {Id=courtId??0 });
                return Json(data);
            }
            catch(Exception ex)
            {
                return Json(new List<CourtDto>());
            }
        }
        [HttpGet]
        
        public async Task<JsonResult> GetOnrs()
        {
            var data = await _mediator.Send(new GetCourtComboQuery { Name=string.Empty });
            return Json(data.ToList());
        }
        [HttpGet]
        public async Task<string> GetOnrDetails(int? onrId)
        {
            string ret = string.Empty;
            var onrs = await _mediator.Send(new GetCourtByIdQuery { Id = onrId ?? 0 });
            if (onrs != null)
            {
               // ret += $"{onrs?.Nasme?.NasmeName},област {onrs?.Nasme?.MunicipalityDtos?.RegionDtos?.Name}, община {onrs?.Nasme?.MunicipalityDtos?.Name}, {onrs?.Address}, имейл:{onrs?.Email}, тел.{onrs?.Phone}";
            }
            return ret;
        }

        public async Task<IActionResult> OnrDetails(int? onrId)
        {
            ViewBag.CourtId = onrId ?? 0;
            var school = await _mediator.Send(new GetCourtByIdQuery { Id = onrId ?? 0 });
            _ = int.TryParse(User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value?.ToString(), out int UserId);
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Преглед на {school?.Name} от {User.Identity.Name}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = UserId, MsgId = 0, Msg = logmsg, IP = ip });
            return View(school);
        }
        [HttpGet]
        public PartialViewResult AddOwnerPartial()
        {
         
                return PartialView("AddOwnerPartial", new CourtDto { Id = 0, CourtGuid = Guid.NewGuid().ToString("N") });
            
           
        }
       

    }
}
