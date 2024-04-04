using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class MainPeriodItem
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? Nmonth { get; set; }
        public int? Nyear { get; set; }
        public int? MetricsFieldId { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredOn { get; set; }
        public DateTime? Datum { get; set; }
        public virtual Court? Court { get; set; }
        public virtual MetricsField? MetricsField { get; set; }
    }
}
