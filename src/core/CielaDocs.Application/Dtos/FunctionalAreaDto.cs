using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class FunctionalAreaDto : IMapFor<FunctionalArea>
    {
        public FunctionalAreaDto()
        {
            FunctionalSubAreasDto = new HashSet<FunctionalSubAreaDto>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<FunctionalSubAreaDto> FunctionalSubAreasDto { get; set; }
    }
}
