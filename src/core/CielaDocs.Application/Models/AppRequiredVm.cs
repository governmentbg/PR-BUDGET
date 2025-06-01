using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public  class AppRequiredVm
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public int InstitutionTypeId { get; set; }
        public bool IsActive { get; set; }
        public string InstitutionTypeName { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
    }
}
