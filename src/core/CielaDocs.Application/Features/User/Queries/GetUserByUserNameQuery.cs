using AutoMapper;
using AutoMapper.QueryableExtensions;


using CielaDocs.Application.Common.Interfaces;
using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CielaDocs.Application.Dtos;

namespace CielaDocs.Application
{
    public class GetUserByUserNameQuery : IRequest<UserDto> { public string UserName { get; set; } }

    public class GetUserByUserNameQueryHandler : IRequestHandler<GetUserByUserNameQuery, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUserByUserNameQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<UserDto> Handle(GetUserByUserNameQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Users
                 .Where(x => x.FirstName == request.UserName)
                   .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                          .FirstOrDefaultAsync(cancellationToken);
            return result;
        }
    }
}