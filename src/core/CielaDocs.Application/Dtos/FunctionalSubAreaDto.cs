using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
   public class FunctionalSubAreaDto:IMapFor<FunctionalSubArea>
    {
        public FunctionalSubAreaDto()
        {
            MainIndicatorsDto = new HashSet<MainIndicatorsDto>();

        }

        public int Id { get; set; }
        public int? FunctionalAreaId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public virtual FunctionalAreaDto? FunctionalAreaDto { get; set; }
        public virtual ICollection<MainIndicatorsDto> MainIndicatorsDto { get; set; }
    }
}
