using AutoMapper;
using AutoMapper.QueryableExtensions;

using CielaDocs.Application.Common.Interfaces;
using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CielaDocs.Application.Dtos;

namespace CielaDocs.Application
{
    public class GetUserExtByIdQuery : IRequest<UserDtoExt> { public int Id { get; set; } }

    public class GetUserExtByIdQueryHandler : IRequestHandler<GetUserExtByIdQuery, UserDtoExt>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUserExtByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<UserDtoExt> Handle(GetUserExtByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Users
                     .Where(x => x.Id == request.Id)
                    
                       .ProjectTo<UserDtoExt>(_mapper.ConfigurationProvider)
                              .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex) { return new UserDtoExt(); }

        }


    }
}