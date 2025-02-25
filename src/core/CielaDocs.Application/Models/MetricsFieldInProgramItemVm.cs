using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
   public class MetricsFieldInProgramItemVm
    {
        public int Id { get; set; }
        public int? MetricsFieldInProgramId { get; set; }
        public int? MainIndicatorsId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? CourtId { get; set; }
        public int? NMonth { get; set; }
        public int? NYear { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? Datum { get; set; }
        public DateTime? EnteredOn { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
