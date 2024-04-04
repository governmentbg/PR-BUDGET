using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class FinYearDto:IMapFor<FinYear>
    {
        public int Id { get; set; }
        public int Nyear { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsInit { get; set; }
    }
}
