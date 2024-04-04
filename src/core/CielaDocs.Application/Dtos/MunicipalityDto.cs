using AutoMapper;

using CielaDocs.Application.Common.Mappings;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class MunicipalityDto : IMapFrom<Municipality>
    {
        public string Id { get; set; }
        /// <summary>
        /// указател към Town-ID - населено място
        /// </summary>
        public string Ekatte { get; set; }
        /// <summary>
        /// Община - наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Идентификатор на община
        /// </summary>
        public int? MunId { get; set; }
        /// <summary>
        /// Идентификатор на област
        /// </summary>
        public int? RegId { get; set; }

     
            public void Mapping(Profile profile)
        {

            profile.CreateMap<Municipality, MunicipalityDto>()          
            .ReverseMap();
        }
    }
}
