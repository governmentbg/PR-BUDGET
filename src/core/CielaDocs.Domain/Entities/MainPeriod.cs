using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class MainPeriod
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? Nmonth { get; set; }
        public int? Nyear { get; set; }
        public int? MainIndicatorsId { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredOn { get; set; }
        public DateTime? Datum { get; set; }
        public decimal? EnteredValue { get; set; }
        public virtual Court? Court { get; set; }
        public virtual MainIndicators? MainIndicators { get; set; }
    }
}
