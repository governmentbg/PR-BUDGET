using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class MainDataDto : IMapFor<MainData>
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int? Nmonth { get; set; }
        public int? Nyear { get; set; }
        public int? MainIndicatorsId { get; set; }
        public decimal? Nvalue { get; set; }
        public DateTime? EnteredOn { get; set; }
        public DateTime? Datum { get; set; }
        public virtual CourtDto? CourtDto { get; set; }
        public virtual MainIndicatorsDto? MainIndicatorsDto { get; set; }
    }
}
