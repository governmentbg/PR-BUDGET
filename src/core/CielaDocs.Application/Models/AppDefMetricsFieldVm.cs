using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
   public class AppDefMetricsFieldVm
    {
        public int Id { get; set; }
        public int? AppDefId { get; set; }
        public int? MetricsField { get; set; }
        public string MetricsFieldName { get; set; } // Added for display purposes
        public string MetricsFieldCode { get; set; } // Added for display purposes
        public bool IsActive { get; set; } = true; // Default to true, can be overridden    
    }
}
