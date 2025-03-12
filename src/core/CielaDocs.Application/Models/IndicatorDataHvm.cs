using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class IndicatorDataHVm
    {
        public int Id { get; set; }

        public int? BudgetPeriodId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? MeasureId { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string Calculation { get; set; }
        public int? PlannedYear1 { get; set; }
        public int? PlannedYear2 { get; set; }
        public int? PlannedYear3 { get; set; }
        public int? PlannedYear4 { get; set; }
        public int? PlannedYear5 { get; set; }
        public decimal? Nvalue1 { get; set; }
        public decimal? Nvalue2 { get; set; }
        public decimal? Nvalue3 { get; set; }
        public decimal? Nvalue4 { get; set; }
        public decimal? Nvalue5 { get; set; }
    }
    public class IndicatorDataCourtHVm
    {
        public int Id { get; set; }
        public int? BudgetPeriodId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int CourtId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? MeasureId { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string Calculation { get; set; }
        public int? PlannedYear1 { get; set; }
        public int? PlannedYear2 { get; set; }
        public int? PlannedYear3 { get; set; }
        public int? PlannedYear4 { get; set; }
        public int? PlannedYear5 { get; set; }
        public decimal? Nvalue1 { get; set; }
        public decimal? Nvalue2 { get; set; }
        public decimal? Nvalue3 { get; set; }
        public decimal? Nvalue4 { get; set; }
        public decimal? Nvalue5 { get; set; }
    }
}
