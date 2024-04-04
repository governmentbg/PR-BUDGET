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
    public class UserDtoExt : IMapFrom<User>
    {
        public UserDtoExt()
        {

        }
        public int Id { get; set; }
        public int CourtId { get; set; }
        [Display(Name = "Име")]
        public string FirstName { get; set; }
        [Display(Name = "Презиме")]
        public string MiddleName { get; set; }
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }
        [Display(Name = "Идентификатор №")]
        public string Identifier { get; set; }
        public string Email { get; set; }
        public bool LoginEnabled { get; set; }
        [Display(Name = "Активен")]
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string PublicEduNumber { get; set; }
        public int? SysUserId { get; set; }
        [Display(Name = "Имейл адрес")]
        [MaxLength(250)]
        public string UserName { get; set; }
       
        [Display(Name = "Тип")]
        public int? UserTypeId { get; set; }
       
        public string AspNetUserId { get; set; }
        [Display(Name = "Име")]
        [MaxLength(1024)]
        public string UserNameExt { get; set; }

     
        [Display(Name = "Тип")]
        public virtual UserTypeDto UserTypeDtos { get; set; }
       
        public int? SchoolBoyId { get; set; }
        [Display(Name = "Име")]
        public string UserFullName => (UserTypeId != 4) ? FirstName + " " + MiddleName + " " + LastName : UserNameExt;
        public string Classes { get; set; }
        public string SubjectSubjectTypes { get; set; }
        [DisplayName("Парола")]
        [StringLength(50)]
        public string Password { get; set; }

        public void Mapping(Profile profile)
        {


            profile.CreateMap<UserType, UserTypeDto>();
            profile.CreateMap<User, UserDtoExt>();
   

        }
    }
}
