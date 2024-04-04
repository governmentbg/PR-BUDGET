using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    public partial class MainDataItem
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
