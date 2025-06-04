using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class AppInputSummarizedCommonVm
    {
        public int Id { get; set; }
        public int? CreatedByInstTypeId { get; set; }
        public int? MetricsFieldId { get; set; }
        public string? MetricsFieldCode { get; set; }
        public string? MetricsFieldName { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? Nval1 { get; set; }
        public decimal? Nval2 { get; set; }
        public decimal? Nval3 { get; set; }
        public decimal? Nval4 { get; set; }

    }
}
