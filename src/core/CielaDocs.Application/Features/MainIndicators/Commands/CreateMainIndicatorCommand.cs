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
    public class CreateMainIndicatorCommand : IRequest<int>
    {

        public int FunctionalSubAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int MeasureId { get; set; }
        public bool IsActive { get; set; }
        public int TypeOfIndicatorId { get; set; }
        public string Calculation { get; set; }
        public string GKey { get; set; }

    }

    public class CreateMainIndicatorCommandHandler : IRequestHandler<CreateMainIndicatorCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateMainIndicatorCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<int> Handle(CreateMainIndicatorCommand request, CancellationToken cancellationToken)
        {
            var exists = _context.MainIndicators.Where(x => x.Code == request.Code && x.FunctionalSubAreaId==request.FunctionalSubAreaId && x.TypeOfIndicatorId==request.TypeOfIndicatorId && x.Name==request.Name).FirstOrDefault();
            if (exists == null)
            {
                var entry = new MainIndicators
                {
                   FunctionalSubAreaId =request?.FunctionalSubAreaId??0,
                   Code =request?.Code,
                   Name =request?.Name,
                   MeasureId =request?.MeasureId ?? 0,
                   IsActive =request?.IsActive ?? false,
                   TypeOfIndicatorId =request?.TypeOfIndicatorId ?? 0,
                   Calculation=request?.Calculation??string.Empty,
                   Gkey=request?.GKey ?? Guid.NewGuid().ToString("N"),
                };
                _context.MainIndicators.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            else return exists.Id;
        }
    }
}




