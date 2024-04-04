using AutoMapper;

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
    public class CourtTypeDto:IMapFor<CourtType>
    {
        public CourtTypeDto() { }
        public int Id { get; set; }
        [DisplayName("Наименование")]
        [StringLength(250)]
        public string Name { get; set; }
        public int InstitutionTypeId { get; set; }
       
    }
}
