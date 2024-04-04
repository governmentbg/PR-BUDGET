using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class KontoCourtsYearVm
    {
        public int Id { get; set; }
        public int NMonth { get; set; }
        public int NYear { get; set; }
        public decimal? NValue { get; set; }
        public string CourtName { get; set; }
        public string ProgramName { get; set; }
        public string RowName { get; set; }
    }
}
