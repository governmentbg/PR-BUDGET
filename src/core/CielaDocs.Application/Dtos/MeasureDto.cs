using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class MeasureDto:IMapFor<Measure>
    {
        public MeasureDto()
        {
            MainIndicatorsDto = new HashSet<MainIndicatorsDto>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<MainIndicatorsDto> MainIndicatorsDto { get; set; }
    }
}
