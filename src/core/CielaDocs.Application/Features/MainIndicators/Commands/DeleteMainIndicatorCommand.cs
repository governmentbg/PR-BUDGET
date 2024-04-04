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
    public class DeleteMainIndicatorCommand : IRequest<bool>
    {
        public int Id { get; set; }

    }
    public class DeleteMainIndicatorCommandHandler : IRequestHandler<DeleteMainIndicatorCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteMainIndicatorCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> Handle(DeleteMainIndicatorCommand request, CancellationToken cancellationToken)
        {
            bool ok = false;
            var entity = await _context.MainIndicators.FindAsync(request.Id);
            if (entity != null)
            {
                try
                {
                    _context.MainIndicators.Remove(entity);
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
