using CielaDocs.Application.Common.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    public abstract class CommonController : Controller
    {
        private IMediator _mediator;
        private ICurrentEmpl _currentEmpl;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
        protected ICurrentEmpl CurrentEmpl => _currentEmpl ??= HttpContext.RequestServices.GetRequiredService<ICurrentEmpl>();
    }
}
