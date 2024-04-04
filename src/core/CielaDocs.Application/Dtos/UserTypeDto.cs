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
    public class UserTypeDto : IMapFrom<UserType>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserType, UserTypeDto>();
        }
    }
}
