using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class KontoRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal? Value { get; set; }
    }
    public class DraftBudgetRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public decimal? Value3 { get; set; }
    }
}
