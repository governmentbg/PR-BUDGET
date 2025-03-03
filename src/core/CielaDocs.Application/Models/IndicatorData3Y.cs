using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class IndicatorData3Y
    {
        public int Id { get; set; }
        public int? MainIndicatorId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? MeasureId { get; set; }
        public bool IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string Calculation { get; set; }
        public string MeasureName { get; set; }
        public string TypeOfIndicatorName { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? Nval1 { get; set; }
        public decimal? Nval2 { get; set; }
        public decimal? Nval3 { get; set; }
        public decimal? Nval4 { get; set; }
    }
}
