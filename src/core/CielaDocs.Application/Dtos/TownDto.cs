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
    public class TownDto:IMapFrom<Town>
    {
        public TownDto()
        {
           
        }
        public string Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Oblast { get; set; }
        public string Obstina { get; set; }
        public string Kmetstvo { get; set; }
        public string NasmeName => Prefix + Name;

        public void Mapping(Profile profile)
        {
      
          
            profile.CreateMap<Town, TownDto>()
            .ReverseMap();
        }
    }
}
