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
using System.Xml.Linq;

namespace CielaDocs.Application
{
    public class CreateCourtCommand : IRequest<int>
    {

        public int Num { get; set; }
        public int CourtTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CourtGuid { get; set; }
        public string KontoCode { get; set; }

    }

    public class CreateCourtCommandHandler : IRequestHandler<CreateCourtCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateCourtCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<int> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
        {
            var exists = _context.Courts.Where(x => x.Name == request.Name).FirstOrDefault();
            if (exists == null)
            {
                var entry = new Court
                {

                    Num = request?.Num ?? 0,
                    CourtTypeId = request?.CourtTypeId ?? 0,
                    Name = request?.Name ?? string.Empty,
                    IsActive = request?.IsActive ?? false,
                    CourtGuid = !string.IsNullOrWhiteSpace(request?.CourtGuid) ? request?.CourtGuid : Guid.NewGuid().ToString("N"),
                    KontoCode=request?.KontoCode ?? string.Empty,
                };
                _context.Courts.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            else return exists.Id;
        }
    }
}



