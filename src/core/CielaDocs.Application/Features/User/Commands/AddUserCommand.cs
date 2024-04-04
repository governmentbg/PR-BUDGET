using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Entities;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.Application
{
    public class AddUserCommand : IRequest<int>
    {

        public string AspNetUserId { get; set; }
        public string Email { get; set; }
       

    }

    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public AddUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<int> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            var exists = _context.Users.Where(x => x.UserName == request.Email).FirstOrDefault();
            if (exists == null)
            {
                var entry = new User
                {

                    FirstName = String.Empty,
                    MiddleName = String.Empty,
                    LastName = String.Empty,
                    Identifier = String.Empty,
                    Email = String.Empty,
                    UserName = request.Email,
                    LoginEnabled = true,
                    LastModifiedOn = DateTime.Now,
                    LastModifiedBy = 0,
                    CreatedOn = DateTime.Now,
                    UserTypeId = 0,
                    CourtId =  0,
                    AspNetUserId = request?.AspNetUserId ?? String.Empty,
                    CanAdd =  false,
                    CanUpdate =  false,
                    CanDelete =  false
                };
                _context.Users.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            else return exists.Id;
        }
    }
}



