using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class AppInputCourt3YVm
    {
        public int Id { get; set; }
        public int? AppId { get; set; }
        public int? CourtId { get; set; }
        public int? MetricsFieldId { get; set; }
        public int? PlannedYear { get; set; }
        public DateTime? EnteredDate { get; set; }
        public string? MetricsFieldCode { get; set; }
        public string? MetricsFieldName { get; set; }
        public decimal? Nval1 { get; set; }
        public decimal? Nval2 { get; set; }
        public decimal? Nval3 { get; set; }
        public decimal? Nval4 { get; set; }
    }
}
