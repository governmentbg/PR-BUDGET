using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
   public class KontoMonthDataLocked
    {
        public int Id { get; set; }
        public int? Nmonth { get; set; }
        public int? Nyear { get; set; }
        public int? LockedBy { get; set; }
        public DateTime? LockedOn { get; set; }
    }
}
