using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class FunctionalArea
    {
        public FunctionalArea()
        {
            FunctionalSubAreas = new HashSet<FunctionalSubArea>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<FunctionalSubArea> FunctionalSubAreas { get; set; }
    }
}
