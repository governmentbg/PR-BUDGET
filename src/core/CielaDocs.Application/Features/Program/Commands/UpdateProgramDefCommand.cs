using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Entities;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace CielaDocs.Application
{
    public class UpdateProgramDefCommand : IRequest
    {
        public int Id { get; set; }
        public int? FunctionalAreaId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? FunctionalActionId { get; set; }
        public int? RowNum { get; set; }
        public string? RowCode { get; set; }
        public string? PrnCode { get; set; }
        public string? Name { get; set; }
        public int? ParentRowNum { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
        public int? CurrencyId { get; set; }
        public int? CurrencyMeasureId { get; set; }
        public DateTime? Datum { get; set; }
        public bool? ValueAllowed { get; set; }
        public int? Num { get; set; }
        public bool? IsActive { get; set; }
        public int? OrderNum { get; set; }
        public string? KontoCodes { get; set; }
        public string? Notes { get; set; }
        public bool? IsCalculated { get; set; }
        public string? ProgCode { get; set; }

    }
    public class UpdateProgramDefCommandHandler : IRequestHandler<UpdateProgramDefCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProgramDefCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Unit> Handle(UpdateProgramDefCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.ProgramDef.FindAsync(request.Id);
            if (entity != null)
            {
                entity.FunctionalAreaId = request?.FunctionalAreaId ?? 0;
                entity.FunctionalSubAreaId = request?.FunctionalSubAreaId ?? 0;
                entity.FunctionalActionId = request?.FunctionalActionId ?? 0;
                entity.RowNum = request?.RowNum ?? 0;
                entity.RowCode = request?.RowCode ?? string.Empty;
                entity.PrnCode = request?.PrnCode ?? string.Empty;
                entity.Name = request?.Name;
                entity.ParentRowNum = request?.ParentRowNum ?? 0;
                entity.Nvalue = request?.Nvalue ?? 0;
                entity.EnteredDate = request?.EnteredDate ?? null;
                entity.CurrencyId = request?.CurrencyId ?? 0;
                entity.CurrencyMeasureId = request?.CurrencyMeasureId ?? 0;
                entity.Datum = request?.Datum ?? DateTime.Now;
                entity.ValueAllowed = request?.ValueAllowed ?? false;
                entity.Num = request?.Num ?? 0;
                entity.IsActive = request?.IsActive ?? false;
                entity.OrderNum = request?.OrderNum ?? 0;
                entity.KontoCodes = request?.KontoCodes ?? string.Empty;
                entity.Notes = request?.Notes ?? string.Empty;
                entity.IsCalculated = request?.IsCalculated ?? false;
                entity.ProgCode = request?.ProgCode ?? string.Empty;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
