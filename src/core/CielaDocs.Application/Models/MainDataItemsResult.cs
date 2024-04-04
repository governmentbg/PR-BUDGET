using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class MainDataItemsResult
    {
        //public int? MainDataId { get; set; }
        //public int? MainIndicatorsId { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string MetricsFieldName { get; set; }
        public decimal Nvalue { get; set; }
    }
}
