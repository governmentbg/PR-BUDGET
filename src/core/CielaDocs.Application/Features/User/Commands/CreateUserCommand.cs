using CielaDocs.Domain.Entities;
using CielaDocs.Application.Common.Exceptions;
using CielaDocs.Application.Common.Interfaces;
using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.Application
{
    public class CreateUserCommand : IRequest<int>
    {
       
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Identifier { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool? LoginEnabled { get; set; }
        public int? UserTypeId { get; set; }
        public int? CourtId { get; set; }

        public string AspNetUserId { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }

    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var exists = _context.Users.Where(x => x.UserName == request.UserName).FirstOrDefault();
            if (exists == null)
            {
                var entry = new User
                {
                   
                    FirstName = request?.FirstName ?? String.Empty,
                    MiddleName = request?.MiddleName ?? String.Empty,
                    LastName = request?.LastName ?? String.Empty,
                    Identifier = request?.Identifier ?? String.Empty,
                    Email = request?.UserName ?? String.Empty,
                    UserName = request?.UserName ?? String.Empty,
                    LoginEnabled = request?.LoginEnabled ?? false,
                    LastModifiedOn =DateTime.Now,
                    LastModifiedBy=0,
                    CreatedOn= DateTime.Now,
                    UserTypeId = request?.UserTypeId ?? 0,
                    CourtId= request?.CourtId ?? 0,
                    AspNetUserId =request?.AspNetUserId ?? String.Empty,
                    CanAdd = request?.CanAdd ?? false,
                    CanUpdate = request?.CanUpdate ?? false,
                    CanDelete = request?.CanDelete ?? false
                };
                _context.Users.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            else return exists.Id;
        }
    }
}



