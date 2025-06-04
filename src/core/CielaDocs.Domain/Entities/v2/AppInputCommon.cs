using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
    public class AppInputCommon
    {
        public int Id { get; set; }
        public int? CreatedByInstTypeId { get; set; }
        public int? MetricsFieldId { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
    }
}
