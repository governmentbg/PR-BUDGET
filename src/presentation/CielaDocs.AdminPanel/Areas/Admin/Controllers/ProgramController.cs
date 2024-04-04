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
using CielaDocs.Application.Models;
using DocumentFormat.OpenXml.Office2010.Excel;


namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class ProgramController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgramController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
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
        public async Task<PartialViewResult> AddProgramPartial(int? functionalSubAreaId)
        {
            var functionalSubAreaName = await _mediator.Send(new GetFnSubAreaByIdQuery { Id = functionalSubAreaId ?? 0 });
            ViewBag.FnSubName = functionalSubAreaName?.Name ?? string.Empty;
            return PartialView("AddProgramPartialView", new ProgramDefVm { Id = 0, FunctionalSubAreaId = functionalSubAreaId ?? 0  });


        }
        [HttpGet]
        public async Task<PartialViewResult> EditProgramPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател!");
            }
            var mi = await _sjcRepo.GetProgramDefByIdAsync(id??0);
            var functionalSubAreaName = await _mediator.Send(new GetFnSubAreaByIdQuery { Id = mi?.FunctionalSubAreaId ?? 0 });
            ViewBag.FnSubName = functionalSubAreaName?.Name ?? string.Empty;
            return PartialView("AddProgramPartialView", mi);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProgramDefVm record)
        {

            if (ModelState.IsValid)
            {


                if (record?.Id == 0)
                {

                    try
                    {
                        List<CustErrors> errLst = GetCustErrors(record);
                        if (errLst.Count > 0)
                        {
                            string messages = string.Join(";", errLst.Select(x => x.Name));
                            return Json(new { msg = messages, success = false, id = 0 });
                        }


                        CreateProgramDefCommand command = new CreateProgramDefCommand
                        {
                            FunctionalAreaId = record?.FunctionalAreaId ?? 0,
                            FunctionalSubAreaId = record?.FunctionalSubAreaId ?? 0,
                            FunctionalActionId = record?.FunctionalActionId ?? 0,
                            RowNum = record?.RowNum ?? 0,
                            RowCode = record?.RowCode ?? string.Empty,
                            PrnCode = record?.PrnCode ?? string.Empty,
                            Name = record?.Name,
                            ParentRowNum = record?.ParentRowNum ?? 0,
                            Nvalue = record?.Nvalue ?? 0,
                            EnteredDate = record?.EnteredDate ?? null,
                            CurrencyId = record?.CurrencyId ?? 0,
                            CurrencyMeasureId = record?.CurrencyMeasureId ?? 0,
                            Datum = record?.Datum ?? DateTime.Now,
                            ValueAllowed = record?.ValueAllowed ?? false,
                            Num = record?.Num ?? 0,
                            IsActive = record?.IsActive ?? false,
                            OrderNum = record?.OrderNum ?? 0,
                            KontoCodes = record?.KontoCodes ?? string.Empty,
                            Notes = record?.Notes ?? string.Empty,
                            IsCalculated = record?.IsCalculated ?? false,
                            ProgCode= record?.ProgCode ?? string.Empty,
                        };
                        var ret = await _mediator.Send(command);

                        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                        string logmsg = $"Добавен бе нов ред {record?.Name} от {User.GetUserName()} към програма";
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

                    UpdateProgramDefCommand command = new UpdateProgramDefCommand
                    {
                        Id = record?.Id ?? 0,
                        FunctionalAreaId = record?.FunctionalAreaId ?? 0,
                        FunctionalSubAreaId = record?.FunctionalSubAreaId ?? 0,
                        FunctionalActionId = record?.FunctionalActionId ?? 0,
                        RowNum = record?.RowNum ?? 0,
                        RowCode = record?.RowCode ?? string.Empty,
                        PrnCode = record?.PrnCode ?? string.Empty,
                        Name = record?.Name,
                        ParentRowNum = record?.ParentRowNum ?? 0,
                        Nvalue = record?.Nvalue ?? 0,
                        EnteredDate = record?.EnteredDate ?? null,
                        CurrencyId = record?.CurrencyId ?? 0,
                        CurrencyMeasureId = record?.CurrencyMeasureId ?? 0,
                        Datum = record?.Datum ?? DateTime.Now,
                        ValueAllowed = record?.ValueAllowed ?? false,
                        Num = record?.Num ?? 0,
                        IsActive = record?.IsActive ?? false,
                        OrderNum = record?.OrderNum ?? 0,
                        KontoCodes = record?.KontoCodes ?? string.Empty,
                        Notes = record?.Notes ?? string.Empty,
                        IsCalculated = record?.IsCalculated ?? false,
                        ProgCode= record?.ProgCode ?? string.Empty

                    };
                    var ret = await _mediator.Send(command);
                    _ = await _sjcRepo.sp_UpdateProgramsByProgramDefAsync(record?.Id ?? 0);
                    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    string logmsg = $"Редактирани данни за програма {record?.Name} от {User.GetUserName()}";
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
        List<CustErrors> GetCustErrors(ProgramDefVm data)
        {

            List<CustErrors> ret = new List<CustErrors>();

            if (string.IsNullOrWhiteSpace(data?.Name))
            {
                ret.Add(new CustErrors { Name = "Невъведено име на ред от програма!" });
            }
           
            return ret;
        }



    }
}
