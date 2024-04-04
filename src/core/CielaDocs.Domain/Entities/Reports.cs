using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class Reports
    {
        public int Id { get; set; }
        public string ReportGuid { get; set; }
        public string ReportCondition { get; set; }
        public string ReportTitle { get; set; }
        public string ReportSubTitle { get; set; }
    }
}
