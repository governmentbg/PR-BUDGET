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
    public class GetCourtByGuidQuery : IRequest<CourtDto>
    {
        public string CourtGuid { get; set; }
    }
    public class GetCourtByGuidQueryHandler : IRequestHandler<GetCourtByGuidQuery, CourtDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetCourtByGuidQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<CourtDto> Handle(GetCourtByGuidQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Courts
                     .Where(x => x.CourtGuid == request.CourtGuid)
                     .ProjectTo<CourtDto>(_mapper.ConfigurationProvider)
                     .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                return new CourtDto();
            }
        }
    }
}