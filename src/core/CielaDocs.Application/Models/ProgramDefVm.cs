using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class ProgramDefVm
    {
        public int Id { get; set; }
        public int? FunctionalAreaId { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public int? FunctionalActionId { get; set; }
        public int? RowNum { get; set; }
        [MaxLength(50)]
        public string? RowCode { get; set; }
        [MaxLength(50)]
        public string? PrnCode { get; set; }
        [MaxLength(1024)]
        public string? Name { get; set; }
        public int? ParentRowNum { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredDate { get; set; }
        public int? CurrencyId { get; set; }
        public int? CurrencyMeasureId { get; set; }
        public DateTime? Datum { get; set; }
        public bool? ValueAllowed { get; set; }
        public int? Num { get; set; }
        public bool? IsActive { get; set; }
        public int? OrderNum { get; set; }
        public string? KontoCodes { get; set; }
        public string? Notes { get; set; }
        public bool? IsCalculated { get; set; }
        public string? ProgCode { get; set; }
    }
}
