using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class DraftBudgetDataVm
    {
       public int Id { get; set; }
      public int? CourtId { get; set; }
      public int? FunctionalSubAreaId { get; set; }
      public int RowNum { get; set; }
      public int? NYear { get; set; }
      public decimal? Nvalue { get; set; }
      public int? CurrencyId { get; set; }
      public string Par { get; set; }
      public int? CurrencyMeasureId { get; set; }
      public DateTime? Datum { get; set; }
      public string Code { get; set; }
      public string RowCode { get; set; }
      public int? ParentRowNum { get; set; }
        public string? FunctionalSubAreaName { get; set; }
        public string? CurrencyName { get; set; }
        public string? CurrencyMeasureName { get; set; }
        public string? CourtName { get; set; }
    }
}
