using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace CielaDocs.Domain.Entities
{
    public class ProgramDef
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
}
