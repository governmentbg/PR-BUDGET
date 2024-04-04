using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class Measure
    {
        public Measure()
        {
            MainIndicators = new HashSet<MainIndicators>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<MainIndicators> MainIndicators { get; set; }
    }
}
