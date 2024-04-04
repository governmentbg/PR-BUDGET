using AutoMapper;

using CielaDocs.Application.Dtos;
using CielaDocs.Application.Dtos.Email;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Common.Mappings
{
    public class CourtMappingProfile : Profile
    {
        public CourtMappingProfile()
        {
            CreateMap<CourtType, CourtTypeDto>();
            CreateMap<Court, CourtDto>()
           .ForMember(d => d.CourtTypeDtos, opt => opt.MapFrom(src => src.CourtType ));

        }


    }
}
