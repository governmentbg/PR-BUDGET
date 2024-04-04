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
    public class DeleteUserCommand : IRequest<bool>
    {
        public int Id { get; set; }

    }
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            bool ok = false;
            var entity = await _context.Users.FindAsync(request.Id);
            if (entity != null)
            {
                try
                {
                    _context.Users.Remove(entity);
                    ok = true;
                }
                catch (Exception)
                {
                    ok = false;
                }
            }
            await _context.SaveChangesAsync(cancellationToken);
            return ok;
        }
    }
}
