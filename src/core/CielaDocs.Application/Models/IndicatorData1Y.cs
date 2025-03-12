using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class IndicatorData1Y
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
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? ApprovedValue { get; set; }
        public decimal? CalculatedValue { get; set; }
        public int? BudgetPeriodId { get; set; }

    }
}
