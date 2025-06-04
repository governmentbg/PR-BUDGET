using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
    public class AppInput
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? MetricsFieldId { get; set; }
        public int? Nmonth { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
    }
}
