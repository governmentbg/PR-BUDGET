using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public partial class MainIndicators
    {
        public MainIndicators()
        {
            MainData = new HashSet<MainData>();
        }

        public int Id { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int? MeasureId { get; set; }
        public bool? IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string? Calculation { get; set; }
        public string? Gkey { get; set; }

        public virtual FunctionalSubArea? FunctionalSubArea { get; set; }
        public virtual Measure? Measure { get; set; }
        public virtual ICollection<MainData> MainData { get; set; }
    }
}
