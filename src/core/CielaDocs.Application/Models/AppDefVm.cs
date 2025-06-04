using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class AppDefVm
    {
        public int Id { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? AppId { get; set; }
        public int? RowNum { get; set; }
        public string RowCode { get; set; }
        public string Name { get; set; }
        public int? ParentRowNum { get; set; }
        public bool? IsActive { get; set; }
        public int? MeasureId { get; set; }
        public string Formula { get; set; }
        public string AppName { get; set; }
        public string MeasureName { get; set; }
    }
}
