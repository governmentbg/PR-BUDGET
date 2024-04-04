using AutoMapper;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class MainIndicatorsDto : IMapFor<MainIndicators>
    {
        public MainIndicatorsDto()
        {
            MainDataDto = new HashSet<MainDataDto>();
        }

        public int Id { get; set; }
        public int? FunctionalSubAreaId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int? MeasureId { get; set; }
        public bool? IsActive { get; set; }
        public int? TypeOfIndicatorId { get; set; }
        public string? Calculation { get; set; }
        public string? Gkey { get; set; }

        public virtual FunctionalSubAreaDto? FunctionalSubAreaDto { get; set; }
        public virtual MeasureDto? MeasureDto { get; set; }
        public virtual ICollection<MainDataDto> MainDataDto { get; set; }

    }
}