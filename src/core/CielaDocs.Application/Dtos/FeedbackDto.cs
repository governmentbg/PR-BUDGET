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

namespace CielaDocs.Application.Dtos
{
    public class FeedbackDto : IMapFrom<Feedback>
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Въведете Вашето име с минимална дължина от 4 символа", AllowEmptyStrings = false)]
        [DisplayName("Име")]
        [MaxLength(250)]
     
        public string Name { get; set; }
        [Required(ErrorMessage = "Изисква се валиден имейл адрес", AllowEmptyStrings = false)]
        [RegularExpression("\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*", ErrorMessage = "Въведете валиден имейл адрес.")]
        [DisplayName("Имейл адрес")]
        [MaxLength(250)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Въведете текст с минимална дължина от 10 символа", AllowEmptyStrings = false)]
        [DisplayName("Коментар")]
        [MaxLength(500)]
        public string Notes { get; set; }
        public DateTime? Datum { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Feedback, FeedbackDto>()
            .ReverseMap();

        }
    }
}
