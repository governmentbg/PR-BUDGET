using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class FilterMainDataVm
    {
        public int FunctionalSubAreaId { get; set; }
        public int CourtId { get; set; }
        public int Nmonth { get; set; }
        public int Nyear { get; set; }
        public int CurrencyId { get; set; }
        public int CurrencyMeasureId { get; set; }
    }
}
