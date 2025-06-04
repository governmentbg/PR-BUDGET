using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
    public class AppDefMetricsField
    {
        public int Id { get; set; }
        public int? AppDefId { get; set; }
        public int? MetricsField { get; set; }
    }
}
