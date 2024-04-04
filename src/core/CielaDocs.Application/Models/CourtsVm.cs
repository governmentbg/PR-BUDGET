using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class CourtsVm
    {
        public int Id { get; set; }
        public int Num { get; set; }
        public int CourtTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CourtGuid { get; set; }
        public string KontoCode { get; set; }
        public string CourtTypeName { get; set; }
        public string InstitutionTypeName { get; set; }
    }
}
