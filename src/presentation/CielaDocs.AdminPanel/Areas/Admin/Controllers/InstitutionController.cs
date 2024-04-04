using Microsoft.AspNetCore.Mvc;
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
using CielaDocs.Application.Models;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class InstitutionController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstitutionController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
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
        public PartialViewResult AddCourtTypePartial(int? institutionTypeId)
        {

            return PartialView("AddCourtTypePartialView", new CourtTypeVm { Id = 0, InstitutionTypeId = institutionTypeId ?? 0 });


        }
        [HttpGet]
        public async Task<PartialViewResult> EditCourtTypePartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател!");
            }
            var mi = await _sjcRepo.GetCourtTypeVmByIdAsync(id ?? 0);
            return PartialView("AddCourtTypePartialView", mi);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CourtTypeVm record)
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


                        
                        var ret = await _sjcRepo.UpdateCourtTypeAsync(record);

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

                    var ret = await _sjcRepo.UpdateCourtTypeAsync(record);
                 
                    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    string logmsg = $"Редактирани данни за тип съдилище {record?.Name} от {User.GetUserName()}";
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
        List<CustErrors> GetCustErrors(CourtTypeVm data)
        {

            List<CustErrors> ret = new List<CustErrors>();

            if (string.IsNullOrWhiteSpace(data?.Name))
            {
                ret.Add(new CustErrors { Name = "Невъведено име на орган на съдебната власт!" });
            }
            if (data?.InstitutionTypeId < 1) {
                ret.Add(new CustErrors { Name = "Неизбрана принадлежност към орган на съдебната власт!" });
            }
            return ret;
        }



    }
}
