using AutoMapper;
using AutoMapper.QueryableExtensions;

using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Application.Common.Mappings;
using CielaDocs.Application.Dtos;
using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.Application
{
    public class GetUserByAspNetUserIdQuery : IRequest<UserDto> { public string AspNetUserId { get; set; } }

    public class GetUserByAspNetUserIdQueryHandler : IRequestHandler<GetUserByAspNetUserIdQuery, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUserByAspNetUserIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<UserDto> Handle(GetUserByAspNetUserIdQuery request, CancellationToken cancellationToken)
        {

            var result = await _context.Users
                 .Where(x => x.AspNetUserId == request.AspNetUserId)
                   .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                          .FirstOrDefaultAsync(cancellationToken);
            return (result!=null)?result:new UserDto();
        }
    }
}