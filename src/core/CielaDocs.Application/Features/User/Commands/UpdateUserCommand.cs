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
    public class UpdateUserCommand : IRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Identifier { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool LoginEnabled { get; set; }
        public int? UserTypeId { get; set; }
        public int? CourtId { get; set; }

        public string AspNetUserId { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }

    }
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Users.FindAsync(request.Id);
            if (entity != null)
            {
               
                entity.FirstName = request?.FirstName ?? String.Empty;
                entity.MiddleName = request?.MiddleName ?? String.Empty;
                entity.LastName = request?.LastName ?? String.Empty;
                entity.Identifier = request?.Identifier ?? String.Empty;
                entity.Email = request?.Email ?? String.Empty;
                entity.UserTypeId = request?.UserTypeId ?? 0;
                entity.CourtId = request?.CourtId ?? 0;
                entity.LoginEnabled = request?.LoginEnabled ?? false;
                entity.CanAdd= request?.CanAdd ?? false;
                entity.CanUpdate= request?.CanUpdate ?? false;
                entity.CanDelete= request?.CanDelete ?? false;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
