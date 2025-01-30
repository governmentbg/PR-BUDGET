using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class ProgramData3Y
    {
       public int Id { get; set; }
       public int? FunctionalSubAreaId { get; set; }
        public int? RowNum { get; set; }
        public int? PlannedYear { get; set; }
         public string PrnCode { get; set; }
        public string Name { get; set; }
        public bool? ValueAllowed { get; set; }
        public decimal? Nval1 { get; set; }
        public decimal? Nval2 { get; set; }
        public decimal? Nval3 { get; set; }
        public decimal? Nval4 { get; set; }
    }
}
