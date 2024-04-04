using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class CourtTypeVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? InstitutionTypeId { get; set; }
    }
}
