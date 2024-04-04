using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.SjcWeb.Models;

using ClosedXML.Excel;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CielaDocs.SjcWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class LogReportController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;

        public LogReportController(IMediator mediator, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo)
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
       
    }
}
