using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class Metrics
    {
      public int Id { get; set; }
      public string Code { get; set; }
      public string Name { get; set; }
      public string Calculation { get; set; }
      public string GKey { get; set; }
       public int FunctionalSubAreaId { get; set; }
       public virtual FunctionalSubArea FunctionalSubArea { get; set; }
    }
}
