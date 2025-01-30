using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Application.Dtos;
using CielaDocs.Application;
using MediatR;
using CielaDocs.AdminPanel.Extensions;

namespace CielaDocs.AdminPanel.Services
{
    public class CurrentEmplService : ICurrentEmpl
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public CurrentEmplService(IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;

        }
        public async Task<UserDto> GetCurrentEmplAsync()
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = _httpContextAccessor.HttpContext.User.GetUserIdValue() });
            return empl;
        }
       
    }
}
