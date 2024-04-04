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
    public class CreateProgramDefCommand : IRequest<int>
    {

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

    public class CreateProgramDefCommandHandler : IRequestHandler<CreateProgramDefCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateProgramDefCommandHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<int> Handle(CreateProgramDefCommand request, CancellationToken cancellationToken)
        {
            var exists = _context.ProgramDef.Where(x => x.FunctionalSubAreaId == request.FunctionalSubAreaId && x.RowNum == request.RowNum).FirstOrDefault();
            if (exists == null)
            {
                var entry = new ProgramDef
                {
                        FunctionalAreaId=request?.FunctionalAreaId??0,
                        FunctionalSubAreaId=request?.FunctionalSubAreaId??0,
                        FunctionalActionId=request?.FunctionalActionId??0,
                        RowNum=request?.RowNum ?? 0,
                        RowCode=request?.RowCode??string.Empty,
                        PrnCode=request?.PrnCode??string.Empty,
                        Name=request?.Name,
                        ParentRowNum=request?.ParentRowNum ?? 0,
                        Nvalue=request?.Nvalue ?? 0,
                        EnteredDate=request?.EnteredDate??null,
                        CurrencyId=request?.CurrencyId??0,
                        CurrencyMeasureId=request?.CurrencyMeasureId??0,
                        Datum=request?.Datum??DateTime.Now,
                        ValueAllowed=request?.ValueAllowed??false,
                        Num=request?.Num ?? 0,
                        IsActive=request?.IsActive ?? false,
                        OrderNum=request?.OrderNum ?? 0,
                        KontoCodes=request?.KontoCodes??string.Empty,
                        Notes=request?.Notes??string.Empty,
                        IsCalculated=request?.IsCalculated ?? false,
                        ProgCode=request?.ProgCode??string.Empty
                };
                _context.ProgramDef.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            else return exists.Id;
        }
    }
}




