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
    public class UlogMappingProfile : Profile
    {
        public UlogMappingProfile()
        {
            CreateMap<Ulog, UlogDto>();
        }
    }
}
