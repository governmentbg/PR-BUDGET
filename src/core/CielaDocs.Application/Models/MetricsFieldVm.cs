using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class MetricsFieldVm
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? NeededFor { get; set; }
        public bool? IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string? TypeOfIndicatorName { get; set; } 
    }
}
