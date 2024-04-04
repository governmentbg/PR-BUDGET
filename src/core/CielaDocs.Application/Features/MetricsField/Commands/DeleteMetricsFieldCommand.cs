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
    public class DeleteMetricsFieldCommand : IRequest<bool>
    {
        public int Id { get; set; }

    }
    public class DeleteMetricsFieldCommandHandler : IRequestHandler<DeleteMetricsFieldCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteMetricsFieldCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> Handle(DeleteMetricsFieldCommand request, CancellationToken cancellationToken)
        {
            bool ok = false;
            var entity = await _context.MetricsField.FindAsync(request.Id);
            if (entity != null)
            {
                try
                {
                    _context.MetricsField.Remove(entity);
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
