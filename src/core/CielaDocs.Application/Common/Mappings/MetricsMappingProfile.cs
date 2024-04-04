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
    public class MetricsMappingProfile : Profile
    {
        public MetricsMappingProfile()
        {
           CreateMap<FunctionalSubArea, FunctionalSubAreaDto>();
           CreateMap<Metrics, MetricsDto>()
           .ForMember(d => d.FunctionalSubAreaDtos, opt => opt.MapFrom(src => src.FunctionalSubArea));

        }


    }
}
