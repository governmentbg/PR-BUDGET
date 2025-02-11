using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
    public class BudgetPeriod
    {
        public int Id { get; set; }
        public int? Y1 { get; set; }
        public int? Y2  { get; set; }
        public int? Y3 { get; set; }
        public int? Y4 { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsUsable { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public string Note { get; set; }
    }
}
