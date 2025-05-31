using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class CourtInKontoCodeVm
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? Nmonth { get; set; }
        public int? Nyear { get; set; }
        public decimal? Nvalue { get; set; }
        public string? KontoCode { get; set; }
        public string CourtKontoCode { get; set; } = string.Empty;
        public string CourtName { get; set; } = string.Empty;
        public string FunctionalSubAreaName { get; set; } = string.Empty;
    }
}
