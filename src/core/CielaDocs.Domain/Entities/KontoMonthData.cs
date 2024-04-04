using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class KontoMonthData
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? ProgramDefId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? RowNum { get; set; }
        public string?    RowCode { get; set; }
        public int? NMonth { get; set; }
        public int? NYear { get; set; }
        public decimal? Nvalue { get; set; }
        public int? CurrencyId { get; set; }
        public int? CurrencyMeasureId { get; set; }
        public DateTime? Datum { get; set; }
    }
}
