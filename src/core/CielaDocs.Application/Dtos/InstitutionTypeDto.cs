using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class InstitutionTypeDto:IMapFor<InstitutionType>
    {
        public int Id { get; set; }
        [DisplayName("Наименование")]
        [StringLength(250)]
        public string Name { get; set; }
    }
}
