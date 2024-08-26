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
using System.Globalization;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ApplicationLogController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationLogController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });


          
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Преглед на действия на потребители(ApplicationLog) {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = user?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetFullActionLogDataGrid()
        {
           
            try
            {
                var data = await _logRepo.GetApplicationLogAsync();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<ApplicationLogVm>());
            }
        }




    }
}
