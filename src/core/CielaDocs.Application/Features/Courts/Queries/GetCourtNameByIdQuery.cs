using AutoMapper;
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
    public class GetCourtNameByIdQuery : IRequest<string>
    {
        public int Id { get; set; }
    }

    public class GetCourtNameByIdQueryHandler : IRequestHandler<GetCourtNameByIdQuery, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetCourtNameByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<string> Handle(GetCourtNameByIdQuery request, CancellationToken cancellationToken)
        {

            var result = await _context.Courts.Where(x => x.Id == request.Id).Select(x=>x.Name).FirstOrDefaultAsync();
               
            return result;
        }
    }
}

