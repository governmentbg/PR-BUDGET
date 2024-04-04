using CielaDocs.Shared.Repository;

using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CielaDocs.SjcWeb.Controllers
{
    public class DocumentUploaderController : Controller
    {
        private readonly ILogger<DocumentUploaderController> _logger;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly IWebHostEnvironment _env;



        public DocumentUploaderController(ILogger<DocumentUploaderController> logger, IConfiguration configuration,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo, IWebHostEnvironment env)
        {
            _logger = logger;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _env = env;
        }
        public IActionResult Index()
        {

            return View();
        }

    }
}
