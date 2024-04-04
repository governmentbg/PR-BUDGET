using AutoMapper;

using CielaDocs.Application.Dtos;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CielaDocs.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
            CreateMap<FunctionalArea, FunctionalAreaDto>();
            CreateMap<MainIndicators, MainIndicatorsDto>();

            CreateMap<FunctionalSubArea, FunctionalSubAreaDto>()
           .ForMember(d => d.FunctionalAreaDto, opt => opt.MapFrom(src => src.FunctionalArea));

            CreateMap<Measure, MeasureDto>();
         
            CreateMap<MainData, MainDataDto>();
            CreateMap<FinYear, FinYearDto>();

        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
              .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
              .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod("Mapping")
                                 ?? type.GetInterface("IMapFrom`1").GetMethod("Mapping");
                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
