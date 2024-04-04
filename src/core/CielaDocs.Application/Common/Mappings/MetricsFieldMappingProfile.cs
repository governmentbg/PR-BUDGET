using AutoMapper;
using CielaDocs.Application.Dtos;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Common.Mappings
{
    internal class MetricsFieldMappingProfile : Profile
    {
        public MetricsFieldMappingProfile()
        {
            CreateMap<TypeOfIndicator, TypeOfIndicatorDto>();
            CreateMap<MetricsField, MetricsFieldDto>()
            .ForMember(d => d.TypeOfIndicatorDtos, opt => opt.MapFrom(src => src.TypeOfIndicator));

        }


    }
}
