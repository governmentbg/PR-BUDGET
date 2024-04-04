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
    public class MetricsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MetricsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
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


            ViewBag.CourtId = user?.CourtId ?? 0;
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Изчисления {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = user?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
            return View();
        }

        [HttpGet]
        public PartialViewResult AddMetricsPartial()
        {

            return PartialView("AddMetricsPartialView", new MetricsDto { Id = 0, GKey = Guid.NewGuid().ToString("N") });


        }
        [HttpGet]
        public async Task<PartialViewResult> EditMetricsPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател към съдебен орган!");
            }
            var court = await _mediator.Send(new GetMetricsByIdQuery { Id = id ?? 0 });
            return PartialView("AddMetricsPartialView", court);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MetricsDto record)
        {

            if (ModelState.IsValid)
            {
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                if ((!empl.CanAdd) && (!empl.CanUpdate))
                {
                    return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
                }

                //var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });


                //    try
                //    {
                //        List<CustErrors> errLst = GetCustErrors(record);
                //        if (errLst.Count > 0)
                //        {
                //            string messages = string.Join(";", errLst.Select(x => x.Name));
                //            return Json(new { msg = messages, success = false, id = 0 });
                //        }


                //        CreateCourtCommand command = new CreateCourtCommand
                //        {
                //            Name = record?.Name ?? string.Empty,
                //            Num = record?.Num ?? 0,
                //            CourtTypeId = record?.CourtTypeId ?? 0,
                //            IsActive = record?.IsActive ?? false,
                //            CourtGuid = record?.CourtGuid ?? string.Empty
                //        };
                //        var ret = await _mediator.Send(command);

                //        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                //        string logmsg = $"Добавен нов орган на съдебна власт {record?.Name} от {empl?.UserName}";
                //        await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = empl?.Id ?? 0, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Add, Msg = logmsg, IP = ip });

                //        return Json(new { msg = "Добавен бе нов орган на съдебна власт ", success = true, id = ret });
                //    }
                //    catch (Exception ex)
                //    {
                //        string messages = string.Join("; ", ModelState.Values
                //             .SelectMany(x => x.Errors)
                //             .Select(x => x.ErrorMessage));
                //        return Json(new { msg = messages, success = false, id = 0 });
                //    }

                //}
                //else
                //{

                //    UpdateCourtCommand command = new UpdateCourtCommand
                //    {
                //        Id = record?.Id ?? 0,
                //        Name = record?.Name ?? string.Empty,
                //        Num = record?.Num ?? 0,
                //        CourtTypeId = record?.CourtTypeId ?? 0,
                //        IsActive = record?.IsActive ?? false

                //    };
                //    var ret = await _mediator.Send(command);
                //    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                //    string logmsg = $"Редактирани данни за орган на съдебна власт {record?.Name} от {empl?.UserName}";
                //    await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = empl?.Id ?? 0, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Edit, Msg = logmsg, IP = ip });

                //    return Json(new { msg = "Данните бяха редактирани", success = true });
                //}

            }
            return Json(new { msg = "Данните бяха редактирани", success = true });
        }
        List<CustErrors> GetCustErrors(MetricsDto data)
        {

            List<CustErrors> ret = new List<CustErrors>();

            if (string.IsNullOrWhiteSpace(data?.Name))
            {
                ret.Add(new CustErrors { Name = "Невъведено име на изчисление!" });
            }

           
            return ret;
        }




    }
}
