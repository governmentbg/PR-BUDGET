using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public class CourtType
    {
        public int Id { get; set; }
        public string Name { get; set; }
	    public int InstitutionTypeId { get; set; }
        public virtual InstitutionType InstitutionType { get; set; }
    }
}
