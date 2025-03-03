using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
   public class IndicatorDataCourt
    {
        public int Id { get; set; }
        public int? MainIndicatorId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? CourtId { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
        public int? PlannedYear { get; set; }
        public decimal? ApprovedValue { get; set; }
        public decimal? CalculatedValue { get; set; }
        public int? BudgetPeriodId { get; set; }
    }
}
