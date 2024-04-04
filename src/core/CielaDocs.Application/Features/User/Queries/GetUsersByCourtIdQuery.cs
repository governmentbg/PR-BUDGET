using AutoMapper;
using AutoMapper.QueryableExtensions;

using CielaDocs.Application.Common.Interfaces;
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
    public class GetUsersByCourtIdQuery : IRequest<IEnumerable<UserDto>> { public int CourtId { get; set; } }

    public class GetUsersByCourtIdQueryHandler : IRequestHandler<GetUsersByCourtIdQuery, IEnumerable<UserDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUsersByCourtIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<UserDto>> Handle(GetUsersByCourtIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Users
                 .Where(x => x.CourtId == request.CourtId)
                   .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                          .OrderBy(t => t.FirstName)
                          .ToListAsync(cancellationToken);
            return result;

        }


    }
}