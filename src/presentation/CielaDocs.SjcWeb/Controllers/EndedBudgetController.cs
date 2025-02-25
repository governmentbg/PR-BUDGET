using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Controllers
{
    [Authorize]
    public class EndedBudgetController : Controller
    {
        private readonly ILogger<HomeController> _logger;



        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;


        public EndedBudgetController(ILogger<HomeController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IWebHostEnvironment env)
        {
            _logger = logger;
            _mediator = mediator;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _env = env;

        }


        public async Task<IActionResult> Index(string par, int? currencyId)
        {

            int.TryParse(par, out int nYear);

            ViewBag.Nyear = nYear;
            ViewBag.Currency = await _sjcRepo.GetNameByIdFromTable("Currency", currencyId);

            return View();

        }
    }
}
