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
    public class GetUsersExternalQuery : IRequest<IEnumerable<UserDtoExt>> {  }

    public class GetUsersExternalQueryHandler : IRequestHandler<GetUsersExternalQuery, IEnumerable<UserDtoExt>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUsersExternalQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<UserDtoExt>> Handle(GetUsersExternalQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Users
                 .Where(x => x.UserTypeId>4)
                   .ProjectTo<UserDtoExt>(_mapper.ConfigurationProvider)
                          .OrderBy(t => t.FirstName)
                          .ToListAsync(cancellationToken);
            return result;

        }


    }
}