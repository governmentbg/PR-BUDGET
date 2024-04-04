using AutoMapper;
using CielaDocs.Domain.Entities;
using CielaDocs.Application.Common.Exceptions;
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
    public class CheckUserByAspNetUserIdQuery : IRequest<bool> { public string AspNetUserId { get; set; } }

    public class CheckUserByAspNetUserIdQueryHandler : IRequestHandler<CheckUserByAspNetUserIdQuery, bool>
    {
        private readonly IApplicationDbContext _context;

        public CheckUserByAspNetUserIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
 
        }
        public async Task<bool> Handle(CheckUserByAspNetUserIdQuery request, CancellationToken cancellationToken)
        {
             var result = await _context.Users
                 .Where(x => x.AspNetUserId == request.AspNetUserId)
                 .FirstOrDefaultAsync(cancellationToken);
            return result!=null;

        }


    }
}