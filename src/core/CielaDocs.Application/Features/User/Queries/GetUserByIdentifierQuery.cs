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
    public class GetUserByIdentifierQuery : IRequest<UserDto> { public string Identifier { get; set; } }

    public class GetUserByIdentifierQueryHandler : IRequestHandler<GetUserByIdentifierQuery, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUserByIdentifierQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<UserDto> Handle(GetUserByIdentifierQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Users
                 .Where(x => x.Identifier == request.Identifier)
                   .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                          .FirstOrDefaultAsync(cancellationToken);
            return result;
        }
    }
}