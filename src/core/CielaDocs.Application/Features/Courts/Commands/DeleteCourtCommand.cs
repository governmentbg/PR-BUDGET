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
    public class DeleteCourtCommand : IRequest<bool>
    {
        public int Id { get; set; }

    }
    public class DeleteCourtCommandHandler : IRequestHandler<DeleteCourtCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCourtCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> Handle(DeleteCourtCommand request, CancellationToken cancellationToken)
        {
            bool ok = false;
            var entity = await _context.Courts.FindAsync(request.Id);
            if (entity != null)
            {
                try
                {
                    _context.Courts.Remove(entity);
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
