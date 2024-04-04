using AutoMapper;
using CielaDocs.Application.Common.Interfaces;
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
    public class GetCourtsCountQuery : IRequest<int>
    {
     
    }

    public class GetCourtsCountQueryHandler : IRequestHandler<GetCourtsCountQuery, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetCourtsCountQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<int> Handle(GetCourtsCountQuery request, CancellationToken cancellationToken)
        {

            var result = await _context.Courts.CountAsync(cancellationToken);

            return result;
        }
    }
}
