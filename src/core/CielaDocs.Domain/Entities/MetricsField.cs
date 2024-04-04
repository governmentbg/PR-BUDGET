using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class MetricsField
    {
        public MetricsField()
        {
            MainDataItems = new HashSet<MainDataItem>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? NeededFor { get; set; }
        public bool? IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public virtual TypeOfIndicator TypeOfIndicator { get; set; }
        public virtual ICollection<MainDataItem> MainDataItems { get; set; }
    }
}
