using CielaDocs.Application.Common.Mappings;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class ReportsDto:IMapFrom<Reports>
    {
        public int Id { get; set; }
        public string ReportGuid { get; set; }
        public string ReportCondition { get; set; }
        public string ReportTitle { get; set; }
        public string ReportSubTitle { get; set; }
    }
}
