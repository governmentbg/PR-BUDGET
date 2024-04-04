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
    public class GetCourtByIdQuery : IRequest<CourtDto> {
        public int Id { get; set; }
    }

    public class GetCourtByIdQueryHandler : IRequestHandler<GetCourtByIdQuery, CourtDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetCourtByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<CourtDto> Handle(GetCourtByIdQuery request, CancellationToken cancellationToken)
        {

            try
            {
                MapperConfiguration configReg = new MapperConfiguration(cfg => {
                    cfg.AddProfile(new CourtMappingProfile());
                });
                IMapper mapperUpper = new Mapper(configReg);
                var result = await _context.Courts
                     .Where(x => x.Id == request.Id)
                      .Include(x => x.CourtType)
                     .ProjectTo<CourtDto>(configReg)
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

