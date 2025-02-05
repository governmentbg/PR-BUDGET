using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class KontoPbInstitutionTypeLockedVm
    {
        public int Id { get; set; }
        public int? InstitutionTypeId { get; set; }
        public int? Nyear { get; set; }
        public int? LockedBy { get; set; }
        public DateTime? LockedOn { get; set; }
        public string LockedByUserName { get; set; }
        public string Name { get; set; }
    }
}
