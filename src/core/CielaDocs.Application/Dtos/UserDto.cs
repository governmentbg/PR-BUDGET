using AutoMapper;

using CielaDocs.Application.Common.Mappings;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CielaDocs.Application.Dtos
{
    public class UserDto : IMapFrom<User>
    {
        public UserDto()
        {

        }
        public int Id { get; set; }
 
       
        public int CourtId { get; set; }
        [Display(Name = "Име")]
        [MaxLength(250)]
        public string FirstName { get; set; }

        [Display(Name = "Презиме")]
        [MaxLength(250)]
        public string MiddleName { get; set; }
        [Display(Name = "Фамилия")]
        [MaxLength(250)]
        public string LastName { get; set; }
        [Display(Name = "Идентификатор №")]
        [MaxLength(50)]
        public string Identifier { get; set; }
        [MaxLength(250)]
        public string Email { get; set; }

        [Display(Name = "Достъп")]
        public bool LoginEnabled { get; set; }
        public DateTime? CreatedOn { get; set; }
      
      
        [Display(Name = "Имейл адрес")]
        [MaxLength(250)]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }
      
        [Display(Name = "Тип")]
        public int? UserTypeId { get; set; }
       
        public string AspNetUserId { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }


        public virtual UserTypeDto UserTypeDtos { get; set; }
        public virtual CourtDto CourtDtos { get; set; }

        public string UserFullName => FirstName + " " + MiddleName + " " + LastName ;
        [DisplayName("Парола")]
        [StringLength(50)]
        public string Password { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserType, UserTypeDto>()
           .ForMember(d => d.Name, opt => opt.MapFrom(src => src.Name));
            profile.CreateMap<Court, CourtDto>()
         .ForMember(d => d.Name, opt => opt.MapFrom(src => src.Name));
            profile.CreateMap<User, UserDto>()
           .ForMember(d => d.UserTypeDtos, opt => opt.MapFrom(src => src.UserType))
           .ForMember(d => d.CourtDtos, opt => opt.MapFrom(src => src.Court));
        }
    }
}
