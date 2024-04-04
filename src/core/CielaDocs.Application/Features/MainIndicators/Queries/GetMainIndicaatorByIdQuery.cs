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
    public class GetMainIndicatorByIdQuery : IRequest<MainIndicatorsDto>
    {
        public int Id { get; set; }
    }

    public class GetMainIndicatorByIdQueryHandler : IRequestHandler<GetMainIndicatorByIdQuery, MainIndicatorsDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetMainIndicatorByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<MainIndicatorsDto> Handle(GetMainIndicatorByIdQuery request, CancellationToken cancellationToken)
        {

            try
            {
                var result = await _context.MainIndicators
                     .Where(x => x.Id == request.Id)
                     .ProjectTo<MainIndicatorsDto>(_mapper.ConfigurationProvider)
                     .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                return new MainIndicatorsDto();
            }
        }
    }
}


