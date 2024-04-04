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
    public class MetricsFieldController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MetricsFieldController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
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
            string logmsg = $"Входни данни {User?.Identity?.Name}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = user?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
            return View();
        }

        [HttpGet]
        public PartialViewResult AddMetricsFieldPartial()
        {

            return PartialView("AddMetricsFieldPartialView", new MetricsFieldDto { Id = 0 ,TypeOfIndicatorId=3 });


        }
        [HttpGet]
        public async Task<PartialViewResult> EditMetricsFieldPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател към съдебен орган!");
            }
            var court = await _mediator.Send(new GetMetricsFieldByIdQuery { Id = id ?? 0 });
            return PartialView("AddMetricsFieldPartialView", court);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MetricsFieldDto record)
        {

            if (ModelState.IsValid)
            {


                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
               if ((!empl.CanAdd) && (!empl.CanUpdate))
                {
                    return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
                }

                try
                {
                    List<CustErrors> errLst = GetCustErrors(record);
                    if (errLst.Count > 0)
                    {
                        string messages = string.Join(";", errLst.Select(x => x.Name));
                        return Json(new { msg = messages, success = false, id = 0 });
                    }

                    if (record?.Id == 0)
                    {
                        CreateMetricsFieldCommand command = new CreateMetricsFieldCommand
                        {
                            Name = record?.Name ?? string.Empty,
                            Code = record?.Code ?? string.Empty,
                            TypeOfIndicatorId = record?.TypeOfIndicatorId ?? 3,
                            IsActive = record?.IsActive ?? false,
                        };
                        var ret = await _mediator.Send(command);

                        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                        string logmsg = $"Добавен нов запис за входни данни {record?.Name} от {empl?.UserName}";
                        await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = empl?.Id ?? 0, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Add, Msg = logmsg, IP = ip });

                        return Json(new { msg = "Добавен бе нов запис за входни данни ", success = true, id = ret });
                    }
                    else {
                        UpdateMetricsFieldCommand command = new UpdateMetricsFieldCommand
                        {
                            Id = record?.Id ?? 0,
                            Name = record?.Name ?? string.Empty,
                            Code = record?.Code ?? string.Empty,
                            TypeOfIndicatorId = record?.TypeOfIndicatorId ?? 3,
                            IsActive = record?.IsActive ?? false

                        };
                        var ret = await _mediator.Send(command);
                        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                        string logmsg = $"Редактирани данни за входна данна {record?.Name} от {empl?.UserName}";
                        await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = empl?.Id ?? 0, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Edit, Msg = logmsg, IP = ip });

                        return Json(new { msg = "Данните бяха редактирани", success = true });
                    }
                }
                catch (Exception ex)
                {
                    string messages = string.Join("; ", ModelState.Values
                         .SelectMany(x => x.Errors)
                         .Select(x => x.ErrorMessage));
                    return Json(new { msg = messages, success = false, id = 0 });
                }

            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                         .SelectMany(x => x.Errors)
                         .Select(x => x.ErrorMessage));
                return Json(new { msg = messages, success = false, id = 0 });

            }



        }
        List<CustErrors> GetCustErrors(MetricsFieldDto data)
        {

            List<CustErrors> ret = new List<CustErrors>();
            if (string.IsNullOrWhiteSpace(data?.Code))
            {
                ret.Add(new CustErrors { Name = "Невъведен код за входни данни!" });
            }
            if (string.IsNullOrWhiteSpace(data?.Name))
            {
                ret.Add(new CustErrors { Name = "Невъведено име за входни данни!" });
            }


            return ret;
        }




    }
}
