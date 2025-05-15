using CielaDocs.Application.Common.Constants;
using CielaDocs.Application;

using Microsoft.AspNetCore.Mvc;
using CielaDocs.AdminPanel.Extensions;
using AutoMapper;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using MediatR;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]

    public class UtilsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcService _sjcService;
        private readonly ISjcBudgetRepositoryV2 _sjcServicev2;

        public UtilsController(IMediator mediator,
             IMapper mapper,
             ILogRepository logRepo,
             ISjcBudgetRepository sjcRepo,
             IHttpContextAccessor httpContextAccessor,
             ISjcService sjcService,
             ISjcBudgetRepositoryV2 sjcServicev2)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
            _sjcService = sjcService;
            _sjcServicev2 = sjcServicev2;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> NullifyKontoTable()
        {
            //TODO:analize cards->SigId

            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl?.CanDelete != true)
            {
                return Json(new { result = false, msg = CommonConstants.LogMsgForbiddenDel });
            }
            try
            {
                int ny= await _sjcServicev2.GetCurrentYearAsync();
                _ = await _sjcService.ExecuteRawSql($"Update KontoMonthData set Nvalue=0 where Nyear={ny}");
                _ = await _sjcService.ExecuteRawSql($"Update ProgramDataCourt set CalculatedValue=0 where PlannedYear={ny}");


                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Изтриване на заредени данни от Конто от {empl?.UserName}";
                await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = empl?.CourtId ?? 0, EmplId = empl.Id, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Delete, Msg = logmsg, IP = ip });
                return Json(new { result = true, msg = "Данните бяха премахнати" });
            }
            catch (Exception ex) {
                return Json(new { result = false, msg = $"Данните не бяха премахнати. Грешка {ex?.Message}" });
            }
        }
    }
}
