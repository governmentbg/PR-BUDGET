using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Entities;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CielaDocs.Application
{
    public class UpdateMainIndicatorCommand : IRequest
    {
        public int Id { get; set; }
        public int FunctionalSubAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int MeasureId { get; set; }
        public bool IsActive { get; set; }
        public int TypeOfIndicatorId { get; set; }
        public string Calculation { get; set; }
        public string GKey { get; set; }

    }
    public class UpdateMainIndicatorCommandHandler : IRequestHandler<UpdateMainIndicatorCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateMainIndicatorCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Unit> Handle(UpdateMainIndicatorCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.MainIndicators.FindAsync(request.Id);
            if (entity != null)
            {
                entity.FunctionalSubAreaId = request?.FunctionalSubAreaId ?? 0;
                entity.Code = request?.Code;
                entity.Name = request?.Name;
                entity.MeasureId = request?.MeasureId ?? 0;
                entity.IsActive = request?.IsActive ?? false;
                entity.TypeOfIndicatorId = request?.TypeOfIndicatorId ?? 0;
                entity.Calculation = request?.Calculation??string.Empty;
                entity.Gkey = request?.GKey ?? Guid.NewGuid().ToString("N");
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
