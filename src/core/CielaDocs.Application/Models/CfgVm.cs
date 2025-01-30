using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class CfgVm
    {
        public int Id { get; set; }
        public int OfficialCurrencyId { get; set; }
        public string OfficialCurrencyCode { get; set; }
        public decimal OfficialEuroRate { get; set; }
        public int DefaultCurrencyMeasureId { get; set; }

    }
}
