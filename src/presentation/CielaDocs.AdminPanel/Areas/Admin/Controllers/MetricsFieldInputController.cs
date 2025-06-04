using AutoMapper;

using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class MetricsFieldInputController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcService _sjcService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MetricsFieldInputController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcService sjcService, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcService = sjcService;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
       

    }
}
