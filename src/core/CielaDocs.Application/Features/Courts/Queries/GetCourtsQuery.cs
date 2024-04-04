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
    public class GetCourtsQuery : IRequest<IEnumerable<CourtDto>>{}

    public class GetCourtsQueryHandler : IRequestHandler<GetCourtsQuery, IEnumerable<CourtDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetCourtsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CourtDto>> Handle(GetCourtsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Courts
                     .ProjectTo<CourtDto>(_mapper.ConfigurationProvider)
                     .ToListAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                return new List<CourtDto>();
            }
        }
    }
}

