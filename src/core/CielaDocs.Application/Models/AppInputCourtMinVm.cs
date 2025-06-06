using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class AppInputCourtMinVm
    {
        public int Id { get; set; }
        public int? AppId { get; set; }
        public int? CourtId { get; set; }
        public int? MetricsFieldId { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
    }
}
