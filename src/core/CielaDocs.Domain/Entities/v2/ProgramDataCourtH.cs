using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
    public class ProgramDataCourtH
    {
        public int Id { get; set; }
        public int? BudgetPeriodId { get; set; }
        public int? CourtId { get; set; }
        public int? ProgramDefNum { get; set; }
        public int? FunctionalAreaId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? FunctionalActionId { get; set; }
        public int? RowNum { get; set; }
        public string RowCode { get; set; }
        public string PrnCode { get; set; }
        public string Name { get; set; }
        public int? ParentRowNum { get; set; }
        public int? CurrencyId { get; set; }
        public int? CurrencyMeasureId { get; set; }
        public DateTime? Datum { get; set; }
        public int? PlannedYear1 { get; set; }
        public int? PlannedYear2 { get; set; }
        public int? PlannedYear3 { get; set; }
        public int? PlannedYear4 { get; set; }
        public int? PlannedYear5 { get; set; }
        public decimal? Nvalue1 { get; set; }
        public decimal? Nvalue2 { get; set; }
        public decimal? Nvalue3 { get; set; }
        public decimal? Nvalue4 { get; set; }
        public decimal? Nvalue5 { get; set; }
    }
}
