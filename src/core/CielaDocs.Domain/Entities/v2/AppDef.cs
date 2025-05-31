using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CielaDocs.Domain.Entities.v2
{
    public class AppDef
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
    }
}
