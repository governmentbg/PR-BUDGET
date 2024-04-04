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
    public class UpdateMetricsFieldCommand : IRequest
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }

    }

    public class UpdateMetricsFieldCommandHandler : IRequestHandler<UpdateMetricsFieldCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateMetricsFieldCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<Unit> Handle(UpdateMetricsFieldCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.MetricsField.FindAsync(request.Id);
            if (entity != null)
            {
                entity.Code = request?.Code??string.Empty;
                entity.Name = request?.Name;
                entity.IsActive = request?.IsActive ?? false;
                entity.TypeOfIndicatorId = request?.TypeOfIndicatorId ?? 0;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}




