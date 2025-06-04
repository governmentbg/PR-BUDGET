using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class AppInputSummarizedVm
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? MetricsFieldId { get; set; }
        public int? Nmonth { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? CalculatedValue { get; set; }
        public DateTime? EnteredDate { get; set; }
        public string? MetricsFieldCode { get; set; }
        public string? MetricsFieldName { get; set; }
        public string? CourtName { get; set; }
    }
}
