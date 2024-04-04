using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class ProgramDataCourtGridVm
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public int? ProgramDefNum { get; set; }
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
        public int? PlannedYear { get; set; }
        public bool? IsActive { get; set; }
        public int? OrderNum { get; set; }
        public decimal? ApprovedValue { get; set; }
        public decimal? CalculatedValue { get; set; }
        public bool? IsCalculated { get; set; }

        public string? FunctionalSubAreaName { get; set; }
        public string? CurrencyName { get; set; }
        public string? CurrencyMeasureName { get; set; }
        public string? CourtName { get; set; }
    }
}
