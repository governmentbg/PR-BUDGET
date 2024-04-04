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
using Microsoft.AspNetCore.Identity;
using Microsoft.Graph;
using CielaDocs.Domain.Entities;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class MainIndicatorsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MainIndicatorsController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<PartialViewResult> AddMainIndicatorPartial(int? functionalSubAreaId)
        {
            var functionalSubAreaName=await _mediator.Send(new GetFnSubAreaByIdQuery { Id = functionalSubAreaId??0 });
            ViewBag.FnSubName=functionalSubAreaName?.Name??string.Empty;
            return PartialView("AddMainIndicatorPartialView", new MainIndicatorsDto { Id = 0,FunctionalSubAreaId= functionalSubAreaId??0 , Gkey = Guid.NewGuid().ToString("N") });


        }
        [HttpGet]
        public async Task<PartialViewResult> EditMainIndicatorsPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател!");
            }
            var mi = await _mediator.Send(new GetMainIndicatorByIdQuery { Id = id ?? 0 });
            var functionalSubAreaName = await _mediator.Send(new GetFnSubAreaByIdQuery { Id = mi?.FunctionalSubAreaId ?? 0 });
            ViewBag.FnSubName = functionalSubAreaName?.Name ?? string.Empty;
            return PartialView("AddMainIndicatorPartialView", mi);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MainIndicatorsDto record)
        {

            if (ModelState.IsValid)
            {
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                if ((!empl.CanAdd) && (!empl.CanUpdate))
                {
                    return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
                }

                if (record?.Id==0)
                {

                    try
                    {
                        List<CustErrors> errLst = GetCustErrors(record);
                        if (errLst.Count > 0)
                        {
                            string messages = string.Join(";", errLst.Select(x => x.Name));
                            return Json(new { msg = messages, success = false, id = 0 });
                        }


                        CreateMainIndicatorCommand command = new CreateMainIndicatorCommand
                        {
                            FunctionalSubAreaId = record?.FunctionalSubAreaId ?? 0,
                            Code = record?.Code,
                            Name = record?.Name,
                            MeasureId = record?.MeasureId ?? 0,
                            IsActive = record?.IsActive ?? false,
                            TypeOfIndicatorId = record?.TypeOfIndicatorId ?? 0,
                            Calculation=record?.Calculation??string.Empty,
                            GKey=record?.Gkey ?? string.Empty,
                        };
                        var ret = await _mediator.Send(command);

                        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                        string logmsg = $"Добавен бе нов индикатор {record?.Name} от {User.GetUserName()}";
                        await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = User.GetEmplIdValue(), CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Add, Msg = logmsg, IP = ip });

                        return Json(new { msg = "Добавен бе нов орган на съдебна власт ", success = true, id = ret });
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

                    UpdateMainIndicatorCommand command = new UpdateMainIndicatorCommand
                    {
                        Id = record?.Id ?? 0,
                        FunctionalSubAreaId = record?.FunctionalSubAreaId ?? 0,
                        Code = record?.Code,
                        Name = record?.Name,
                        MeasureId = record?.MeasureId ?? 0,
                        IsActive = record?.IsActive ?? false,
                        TypeOfIndicatorId = record?.TypeOfIndicatorId ?? 0,
                        Calculation = record?.Calculation ?? string.Empty,
                        GKey = record?.Gkey ?? string.Empty,

                    };
                    var ret = await _mediator.Send(command);
                    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    string logmsg = $"Редактирани данни за орган на съдебна власт {record?.Name} от {User.GetUserName()}";
                    await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = User.GetEmplIdValue(), CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Edit, Msg = logmsg, IP = ip });

                    return Json(new { msg = "Данните бяха редактирани", success = true });
                }

            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
                return Json(new { msg = messages, success = false });
            }
        }
        List<CustErrors> GetCustErrors(MainIndicatorsDto data)
        {

            List<CustErrors> ret = new List<CustErrors>();

            if (string.IsNullOrWhiteSpace(data?.Name))
            {
                ret.Add(new CustErrors { Name = "Невъведено име на показателя!" });
            }
            if (string.IsNullOrWhiteSpace(data?.Code))
            {
                ret.Add(new CustErrors { Name = "Невъведен код на показателя!" });
            }
            if (data?.FunctionalSubAreaId < -1)
            {
                ret.Add(new CustErrors { Name = "Изберете програма за показателя!" });
            }
            if (data?.TypeOfIndicatorId < -1)
            {
                ret.Add(new CustErrors { Name = "Изберете тип на показателя!" });
            }
            if (data?.MeasureId < -1)
            {
                ret.Add(new CustErrors { Name = "Изберете мярка на показателя!" });
            }
            return ret;
        }



    }
}
