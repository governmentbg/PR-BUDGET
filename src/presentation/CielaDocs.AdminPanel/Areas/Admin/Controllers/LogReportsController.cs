using AutoMapper;

using CielaDocs.AdminPanel.Models;
using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using DocumentFormat.OpenXml.Math;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Org.BouncyCastle.Bcpg.OpenPgp;

using System.Globalization;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class LogReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;

        public LogReportsController(IMediator mediator, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo)
        {
            _mediator = mediator;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> EmplLogGrid(string par) {

            string[] args=par.Split('|');
            int.TryParse(args[0], out int emplId);
            var empl = await _mediator.Send(new GetUserByIdQuery { Id = emplId });
            ViewBag.Title = $"Извършени действия от {empl?.UserFullName??string.Empty} от {args[1]}  до {args[2]}";
            ViewBag.EmplId=emplId;
            ViewBag.StartDate = args[1];
            ViewBag.EndDate = args[2];
            return View();
        }
        public PartialViewResult GetEmplLogReportFilter(int? emplId)
        {
            if ((emplId == null) || (emplId < 1))
            {
                return PartialView("_ErrorPartialView", "Невалиден идентификатор на потребител!");
            }
            EmplLogVm model = new EmplLogVm();
            model.EmplId = emplId ?? 0;
            model.ReportGuid = Guid.NewGuid().ToString("N");
            return PartialView("EmplLogFilterPartialView", model);
        }
        [HttpGet]
        public async Task<JsonResult> GetEmplActionDataGrid(int? emplId,string? startDate, string? endDate  )
        {
            var cultureInfo=new CultureInfo("bg-BG");
            try
            {
                var data = await _logRepo.GetUserLogByEmplAsync(DateTime.Parse(startDate,cultureInfo), DateTime.Parse(endDate,cultureInfo), emplId ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<Ulog>());
            }
        }
    }
}
