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
    public class GetMetricsByIdQuery : IRequest<MetricsDto> { public int Id { get; set; } }

    public class GetMetricsByIdQueryHandler : IRequestHandler<GetMetricsByIdQuery, MetricsDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetMetricsByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<MetricsDto> Handle(GetMetricsByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                MapperConfiguration configReg = new MapperConfiguration(cfg => {
                    cfg.AddProfile(new MetricsMappingProfile());
                });
                IMapper mapperUpper = new Mapper(configReg);
                var result = await _context.Metrics
                     .Where(x => x.Id == request.Id)
                     .Include(x => x.FunctionalSubArea)
                       .ProjectTo<MetricsDto>(configReg)
                              .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex) { return new MetricsDto(); }

        }


    }
}