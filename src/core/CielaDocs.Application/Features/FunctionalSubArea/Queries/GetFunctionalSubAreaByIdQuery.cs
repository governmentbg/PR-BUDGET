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
    public class GetFunctionalSubAreaByIdQuery : IRequest<FunctionalSubAreaDto>
    {
        public int Id { get; set; }
    }

    public class GetFunctionalSubAreaByIdQueryHandler : IRequestHandler<GetFunctionalSubAreaByIdQuery, FunctionalSubAreaDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetFunctionalSubAreaByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<FunctionalSubAreaDto> Handle(GetFunctionalSubAreaByIdQuery request, CancellationToken cancellationToken)
        {

            try
            {

                var result = await _context.FunctionalSubArea
                     .Where(x => x.Id == request.Id)
                      .Include(x => x.FunctionalArea)
                     .ProjectTo<FunctionalSubAreaDto>(_mapper.ConfigurationProvider)
                     .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                return new FunctionalSubAreaDto();
            }
          
            }
    }
}

