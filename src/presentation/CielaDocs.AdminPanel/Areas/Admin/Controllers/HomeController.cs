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

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminOnly")]
    public class HomeController : CommonController
    {

        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
        {

            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {

            var dbUser = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });


            ViewBag.CourtId = dbUser?.CourtId ?? 0;
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Администриране и конфигуриране на системата от {User?.Identity?.Name ?? string.Empty}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = dbUser?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitFinYear(int? id)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            try
            {
                var res = await _sjcRepo.Sp_InitFinYearStage1Async(id ?? 0);
                var res2 = await _sjcRepo.Sp_InitFinYearStage2Async(id ?? 0);
                return Json(new { msg = "Годишната инициализация завърши", success = true });
            }
            catch (Exception ex)
            {
                return Json(new { msg = $"Годишната инициализация завърши с грешка {ex?.Message} ", success = false });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitProgramData(int? id)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            for (int i = 1; i <= 9; i++)
            {
                try
                {
                    var res = await _sjcRepo.Sp_InitProgramDataAsync(i, id ?? 0);

                    var res2 = await _sjcRepo.Sp_InitProgramDataCourtAsync(i, id ?? 0);
                    var res3 = await _sjcRepo.Sp_InitProgramDataInstitutionAsync(i, id ?? 0);
                }
                catch (Exception ex)
                {
                    return Json(new { msg = $"Годишната инициализация на програма {i} за година {id} завърши с грешка {ex?.Message} ", success = false });
                }
            }
            return Json(new { msg = $"Годишната инициализация на програми за  {id} година завърши", success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgramData(int? id)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            for (int i = 1; i <= 9; i++)
            {
                try
                {
                    var res = await _sjcRepo.Sp_UpdateProgramDataAsync(i, id ?? 0);

                    var res2 = await _sjcRepo.Sp_UpdateProgramDataCourtAsync(i, id ?? 0);

                    var res3 = await _sjcRepo.Sp_UpdateProgramDataInstitutionAsync(i, id ?? 0);
                }
                catch (Exception ex)
                {
                    return Json(new { msg = $"Годишната инициализация на програма {i} за година {id} завърши с грешка {ex?.Message} ", success = false });
                }
            }
            return Json(new { msg = $"Годишната инициализация на програми за  {id} година завърши", success = true });
        }
    }
}
