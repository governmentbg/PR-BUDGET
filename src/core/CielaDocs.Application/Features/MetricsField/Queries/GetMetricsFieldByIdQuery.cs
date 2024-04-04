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
    public class GetMetricsFieldByIdQuery : IRequest<MetricsFieldDto> { public int Id { get; set; } }

    public class GetMetricsFieldByIdQueryHandler : IRequestHandler<GetMetricsFieldByIdQuery, MetricsFieldDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetMetricsFieldByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<MetricsFieldDto> Handle(GetMetricsFieldByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                MapperConfiguration configReg = new MapperConfiguration(cfg => {
                    cfg.AddProfile(new MetricsFieldMappingProfile());
                });
                IMapper mapperUpper = new Mapper(configReg);

                var result = await _context.MetricsField
                     .Where(x => x.Id == request.Id)
                       .ProjectTo<MetricsFieldDto>(configReg)
                              .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex) { return new MetricsFieldDto(); }

        }


    }
}