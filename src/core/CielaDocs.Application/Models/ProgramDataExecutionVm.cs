using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class ProgramDataExecutionVm
    {
        public int Id { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? CourtId { get; set; }
        public int? RowNum { get; set; }
        public int? PlannedYear { get; set; }
        public string PrnCode { get; set; }
        public string Name { get; set; }
        public decimal? Nvalue { get; set; }
        public decimal? ApprovedValue { get; set; }
        public decimal? CalculatedValue { get; set; }
        public bool? ValueAllowed { get; set; }
        public string CourtName { get; set; }
    }
}
