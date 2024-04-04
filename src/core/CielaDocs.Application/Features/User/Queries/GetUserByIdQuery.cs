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
    public class GetUserByIdQuery : IRequest<UserDto> { public int Id { get; set; } }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public GetUserByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Users
                     .Where(x => x.Id == request.Id)
                      .Include(x => x.UserType)
                         .Include(x => x.Court)
                       .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                              .FirstOrDefaultAsync(cancellationToken);
                return result;
            }
            catch (Exception ex) { return new UserDto(); }

        }


    }
}