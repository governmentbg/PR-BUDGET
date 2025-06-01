using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities.v2
{
    public class AppRequired
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public int InstitutionTypeId { get; set; }
        public bool IsActive { get; set; }
    }
}
