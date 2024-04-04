using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
   public class FunctionalSubArea
    {
        public FunctionalSubArea()
        {
            MainIndicators = new HashSet<MainIndicators>();
     
        }

        public int Id { get; set; }
        public int? FunctionalAreaId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public virtual FunctionalArea? FunctionalArea { get; set; }
        public virtual ICollection<MainIndicators> MainIndicators { get; set; }

    }
}
