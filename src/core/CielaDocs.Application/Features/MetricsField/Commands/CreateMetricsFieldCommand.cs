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
    public class CreateMetricsFieldCommand : IRequest<int>
    {

         public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }

    }

    public class CreateMetricsFieldCommandHandler : IRequestHandler<CreateMetricsFieldCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateMetricsFieldCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<int> Handle(CreateMetricsFieldCommand request, CancellationToken cancellationToken)
        {
            var exists = _context.MetricsField.Where(x => x.Code == request.Code).FirstOrDefault();
            if (exists == null)
            {
                var entry = new MetricsField
                {
                  
                    Code = request?.Code,
                    Name = request?.Name,
                    IsActive = request?.IsActive ?? false,
                    TypeOfIndicatorId = request?.TypeOfIndicatorId ?? 0,

                };
                _context.MetricsField.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            else return exists.Id;
        }
    }
}





