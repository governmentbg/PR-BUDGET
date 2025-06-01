using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
   public class AppInputFilter
    {
        public int? CourtId { get; set; }
        public int? Nmonth { get; set; }
        public int? PlannedYear { get; set; }
    }
}
