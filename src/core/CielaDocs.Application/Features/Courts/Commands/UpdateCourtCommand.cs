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
    public class UpdateCourtCommand : IRequest
    {
        public int Id { get; set; }
        public int Num { get; set; }
        public int CourtTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string KontoCode { get; set; }

        public class UpdateCourtCommandHandler : IRequestHandler<UpdateCourtCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdateCourtCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<Unit> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Courts.FindAsync(request.Id);
                if (entity != null)
                {

                    entity.Name = request?.Name ?? String.Empty;
                    entity.Num = request?.Num ?? 0;
                    entity.CourtTypeId = request?.CourtTypeId ?? 0;
                    entity.IsActive = request?.IsActive ?? false;
                    entity.KontoCode= request?.KontoCode ?? String.Empty;
                }
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}
