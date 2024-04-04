using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class MainDataGrid
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? Nmonth { get; set; }
        public int? Nyear { get; set; }
        public int? MainIndicatorsId { get; set; }
        public decimal? Nvalue { get; set; }
        public decimal? EnteredValue { get; set; }
        public DateTime? Datum { get; set; }
        public DateTime? EnteredOn { get; set; }
        public string MainIndicatorName { get; set; }
        public string? Code { get; set; }
        public int? MeasureId { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string? Calculation { get; set; }
        public string? MeasureName { get; set; }
        public string? TypeOfIndicatorName { get; set; }
    }
}
