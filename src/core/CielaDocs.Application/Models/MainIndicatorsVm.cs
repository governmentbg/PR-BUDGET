using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class MainIndicatorsVm
    {
        public int Id { get; set; }
        public int FunctionalSubAreaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int MeasureId { get; set; }
        public bool IsActive { get; set; }
        public string Calculation { get; set; }
        public string GKey { get; set; }
        public string FunctionalSubAreaName { get; set; }
        public string MeasureName { get; set; }
        public string TypeOfIndicatorName { get; set; }

    }
}
